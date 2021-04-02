import {
    getPageMods,
    isOmitParentFieldModification,
    isInnerNamespaceModification,
} from "./modification_db";
import { TSCollection } from "../ts_types";
import {
    WikiFunctionCollection,
    WikiFunction,
    WikiStructItem,
    isWikiFunction,
    isWikiStructItem,
} from "../wiki_types";
import { transformDescription } from "./description";
import { transformFunction } from "./function";
import { transformIdentifier } from "./util";
import { transformStructField } from "./struct";

export function transformFunctionCollection(
    wikiClass: WikiFunctionCollection,
    wikiMembers: (WikiFunction | WikiStructItem)[]
): TSCollection {
    const mods = getPageMods(wikiClass.address);

    let membersCopy = [...wikiMembers];

    const omits = mods.filter(isOmitParentFieldModification).map((mod) => mod.omit);

    const innerNamespaces = mods.filter(isInnerNamespaceModification).map((mod) => {
        const namespaceFuncs = membersCopy.filter((f) => f.address.includes(mod.prefix + "."));
        // remove from original funcs
        membersCopy = membersCopy.filter(
            (f) => !f.address.includes(mod.prefix + ".") && f.name !== mod.prefix
        );

        const namespaceRoot = namespaceFuncs.find((f) => f.name === mod.prefix);
        const namespaceFuncsWithoutRoot = namespaceFuncs.filter((f) => f.name !== mod.prefix);

        return {
            identifier: mod.prefix,
            docComment: transformDescription(namespaceRoot?.description ?? ""),
            functions: namespaceFuncsWithoutRoot
                .filter(isWikiFunction)
                .map(transformFunction)
                .map((f) => {
                    f.identifier = f.identifier.replace(`${mod.prefix}.`, "");
                    return f;
                }),
            fields: namespaceFuncsWithoutRoot
                .filter(isWikiStructItem)
                .map(transformStructField)
                .map((f) => {
                    f.identifier = f.identifier.replace(`${mod.prefix}.`, "");
                    return f;
                }),
            innerCollections: [],
            namespace: true,
        } as TSCollection;
    });

    let parent = wikiClass.parent;

    if (parent && omits.length != 0) {
        parent = `Omit<${parent}, ${omits.map((o) => `"${o}"`).join(" | ")}>`;
    }

    return {
        identifier: transformIdentifier(wikiClass.name),
        docComment: transformDescription(wikiClass.description),
        fields: membersCopy.filter(isWikiStructItem).map(transformStructField),
        functions: membersCopy.filter(isWikiFunction).map(transformFunction),
        parent,
        namespace: wikiClass.library,
        innerCollections: innerNamespaces,
    };
}
