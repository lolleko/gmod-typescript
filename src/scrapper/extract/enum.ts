import { WikiPage, WikiEnum, WikiEnumItem, WikiElementKind } from "../../wiki_types";
import { parseMarkup } from "../util";

export function extractEnum(page: WikiPage): WikiEnum {
    const markupObj = parseMarkup(page.markup, {
        stopNodes: ["description", "item"],
    });

    const enumObj = markupObj.enum[0];

    return {
        kind: WikiElementKind.Enum,
        name: page.title,
        description: enumObj.description.trim(),
        realm: enumObj.realm,
        items: enumObj.items ? enumObj.items[0].item.map(itemObjToEnumItem).flat() : [],
        address: page.address,
    };
}

function itemObjToEnumItem(itemObj: any): WikiEnumItem[] {
    let keys: string[];
    if (itemObj.attr.key.includes(" or ")) {
        keys = [...itemObj.attr.key.split("or").map((s: string) => s.trim())];
    } else {
        keys = [itemObj.attr.key];
    }

    return keys.map((key) => ({
        key,
        value: itemObj.attr.value,
        description: itemObj.__text ? itemObj.__text.trim() : "",
    }));
}
