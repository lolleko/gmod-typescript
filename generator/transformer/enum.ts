import { TSEnum, TSEnumField } from '../ts_types';
import { WikiEnum, WikiEnumItem } from '../wiki_types';
import { createRealmString, transformDescription } from './description';
import {
    RenameEnumIndentifierModification,
    getPageMods,
    isRenameEnumIndentifierModification,
    isRenameIndentifierModification,
} from './modification_db';
import { transformIdentifier } from './util';

export function transformEnum(wikiEnum: WikiEnum): TSEnum {
    const compileMembersOnly = !wikiEnum.items.some((item) => item.key.includes('.'));

    const mods = getPageMods(wikiEnum.address);

    let identifier = wikiEnum.name;

    const renameMod = mods.find(isRenameIndentifierModification);
    const renameEnumMods = mods.filter(isRenameEnumIndentifierModification);

    if (renameMod) {
        identifier = renameMod.newName;
    }

    return {
        identifier,
        docComment:
            createRealmString(wikiEnum.realm) + '\n\n' + transformDescription(wikiEnum.description),
        fields: wikiEnum.items.map((item) =>
            transformEnumField(item, compileMembersOnly, renameEnumMods)
        ),
        compileMembersOnly,
    };
}

export function transformEnumField(
    wikiEnumItem: WikiEnumItem,
    compileMembersOnly: boolean,
    renameEnumMods: RenameEnumIndentifierModification[]
): TSEnumField {
    let identifier = wikiEnumItem.key;

    if (renameEnumMods.length > 0) {
        const renameMod = renameEnumMods.find((x) => x.oldName === identifier);
        if (renameMod) identifier = renameMod.newName;
    }

    // Don't have any dots? Just skip it!
    if (compileMembersOnly) {
        const newIdentifier = identifier.split('.')[1];
        if (newIdentifier) identifier = newIdentifier;
    }

    return {
        identifier: transformIdentifier(identifier),
        docComment: transformDescription(wikiEnumItem.description),
        value: wikiEnumItem.value,
    };
}
