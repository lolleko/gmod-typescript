import { TSEnum, TSEnumField } from '../ts_types';
import { WikiEnum, WikiEnumItem } from '../wiki_types';
import { createRealmString, transformDescription } from './description';
import { getPageMods, isRenameIndentifierModification } from './modification_db';
import { transformIdentifier } from './util';

export function transformEnum(wikiEnum: WikiEnum): TSEnum {
    const compileMembersOnly = !wikiEnum.items.some((item) => item.key.includes('.'));

    const mods = getPageMods(wikiEnum.address);

    let identifier = wikiEnum.name;

    const renameMod = mods.find(isRenameIndentifierModification);

    if (renameMod) {
        identifier = renameMod.newName;
    }

    return {
        identifier,
        docComment:
            createRealmString(wikiEnum.realm) + '\n' + transformDescription(wikiEnum.description),
        fields: wikiEnum.items.map((item) => transformEnumField(item, compileMembersOnly)),
        compileMembersOnly,
    };
}

export function transformEnumField(
    wikiEnumItem: WikiEnumItem,
    compileMembersOnly: boolean
): TSEnumField {
    const identifier = compileMembersOnly ? wikiEnumItem.key : wikiEnumItem.key.split('.')[1];
    return {
        identifier: transformIdentifier(identifier),
        docComment: transformDescription(wikiEnumItem.description),
        value: wikiEnumItem.value,
    };
}
