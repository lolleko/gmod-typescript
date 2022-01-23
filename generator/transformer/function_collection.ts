import {
    getPageMods,
    isOmitParentFieldModification,
    isInnerNamespaceModification,
    isAddParentModification,
    isAddFieldModification,
} from './modification_db';
import { TSCollection } from '../ts_types';
import {
    WikiFunctionCollection,
    WikiFunction,
    WikiStructItem,
    isWikiFunction,
    isWikiStructItem,
} from '../wiki_types';
import { transformDescription } from './description';
import { transformFunction } from './function';
import { transformIdentifier } from './util';
import { transformStructField } from './struct';

export function transformFunctionCollection(
    wikiClass: WikiFunctionCollection,
    wikiMembers: (WikiFunction | WikiStructItem)[]
): TSCollection {
    const mods = getPageMods(wikiClass.address);

    let membersCopy = [...wikiMembers];

    const innerNamespaces = mods.filter(isInnerNamespaceModification).map((mod) => {
        const namespaceFuncs = membersCopy.filter((f) => f.address.includes(mod.prefix + '.'));
        // remove from original funcs
        membersCopy = membersCopy.filter(
            (f) => !f.address.includes(mod.prefix + '.') && f.name !== mod.prefix
        );

        const namespaceRoot = namespaceFuncs.find((f) => f.name === mod.prefix);
        const namespaceFuncsWithoutRoot = namespaceFuncs.filter((f) => f.name !== mod.prefix);

        return {
            identifier: mod.prefix,
            docComment: transformDescription(namespaceRoot?.description ?? ''),
            functions: namespaceFuncsWithoutRoot
                .filter(isWikiFunction)
                .map(transformFunction)
                .map((f) => {
                    f.identifier = f.identifier.replace(`${mod.prefix}.`, '');
                    return f;
                }),
            fields: namespaceFuncsWithoutRoot
                .filter(isWikiStructItem)
                .map(transformStructField)
                .map((f) => {
                    f.identifier = f.identifier.replace(`${mod.prefix}.`, '');
                    return f;
                }),
            innerCollections: [],
            namespace: true,
        } as TSCollection;
    });

    const parents = mods.filter(isAddParentModification).map((mod) => mod.parent);

    if (wikiClass.parent) {
        parents.push(wikiClass.parent);
    }

    const omits = mods.filter(isOmitParentFieldModification);

    const parentsModified = parents;

    for (const omit of omits) {
        if (parents.length > 0) {
            const parentIndex = omit.parent ? parentsModified.indexOf(omit.parent) : 0;
            if (parentIndex != -1) {
                parentsModified[parentIndex] = `Omit<${
                    parentsModified[parentIndex]
                }, ${omit.omits.map((o) => `"${o}"`).join(' | ')}>`;
            }
        }
    }

    const addFieldMods = mods.filter(isAddFieldModification).map((afm) => afm.field);

    return {
        identifier: transformIdentifier(wikiClass.name),
        docComment: transformDescription(wikiClass.description),
        fields: membersCopy.filter(isWikiStructItem).map(transformStructField).concat(addFieldMods),
        functions: membersCopy.filter(isWikiFunction).map(transformFunction),
        parent: parentsModified.join(', '),
        namespace: wikiClass.library,
        innerCollections: innerNamespaces,
    };
}
