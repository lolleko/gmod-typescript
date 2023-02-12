import { WikiElementKind, WikiFunctionCollection, WikiPage } from '../../wiki_types';
import { parseMarkup } from '../util';

export function extractClass(page: WikiPage): WikiFunctionCollection {
    const markupObj = parseMarkup(page.markup, {
        stopNodes: ['summary', 'description'],
    });

    if (markupObj.type) {
        const classObj = markupObj.type[0];

        return {
            kind: WikiElementKind.FunctionCollection,
            name: classObj.attr.name,
            parent: classObj.attr.parent,
            description: classObj.summary.trim(),
            library: false,
            address: page.address,
        };
    } else if (markupObj.panel) {
        const classObj = markupObj.panel[0];

        return {
            kind: WikiElementKind.FunctionCollection,
            name: page.title.replace(' ', '_'),
            parent: classObj.parent,
            description: classObj.description ? classObj.description.trim() : '',
            library: false,
            address: page.address,
        };
    }
    return {
        kind: WikiElementKind.FunctionCollection,
        name: page.title.replace(' ', '_'),
        description: '',
        library: false,
        address: page.address,
    };
}
