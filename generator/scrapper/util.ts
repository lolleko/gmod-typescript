import * as parser from 'fast-xml-parser';

export const xmlParserDefaultOptions: parser.X2jOptionsOptional = {
    textNodeName: '__text',
    ignoreAttributes: false,
    attributeNamePrefix: '',
    attrNodeName: 'attr',
    arrayMode: true,
};

export function parseMarkup(markup: string, extraOptions: parser.X2jOptionsOptional = {}) {
    // insert root because wikei pages can have multiple
    markup = `<root>${markup}</root>`;

    markup = markup
        .replace(/&/g, '&amp;')
        .replace(/ < /g, ' &lt; ')
        .replace(/<\?php/g, '&lt;?php')
        .replace(/\?>/g, '?&gt;')
        .replace(/ > /g, ' &gt; ')
        .replace(/ <= /g, ' &lt;= ')
        .replace(/ >= /g, ' &gt;= ')
        .replace(/`/g, '&grave;')
        .replace(/\\"/g, '&quot;')
        .replace('<arg name="default" type="Angle" default">', '<arg name="default" type="Angle">'); // There's a mistake in one of the pages - Whoops!

    const validation = parser.validate(markup);

    if (validation != true) {
        if (validation.err.code != 'InvalidTag') {
            throw new Error(
                `Invalid markup: \n${JSON.stringify(validation.err, undefined, 4)}\n ${markup}`
            );
        }
    }

    const markupObj = parser.parse(markup, {
        ...xmlParserDefaultOptions,
        ...extraOptions,
    });

    return markupObj.root[0];
}
