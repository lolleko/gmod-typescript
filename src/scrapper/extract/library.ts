import { WikiElementKind, WikiFunctionCollection, WikiPage } from "../../wiki_types";
import { parseMarkup } from "../util";

export function extractLibrary(page: WikiPage): WikiFunctionCollection {
    const markupObj = parseMarkup(page.markup, {
        stopNodes: ["summary"],
    });

    const LibraryObj = markupObj.type[0];

    return {
        kind: WikiElementKind.FunctionCollection,
        name: LibraryObj.attr.name,
        parent: LibraryObj.attr.parent,
        description: LibraryObj.summary.trim(),
        library: true,
        address: page.address,
    };
}
