export function printDocComent(comment: string) {
    const commentWithoutAbudnantNewLines = comment
        .replace(/\n{2,}/g, '\n\n')
        .replace(/\n/g, '\n * ');

    return `
/**
 * ${commentWithoutAbudnantNewLines}
 */
`.trim();
}

export function indentStr(str: string, indent: string) {
    return indent + str.replace(/\n/g, `\n${indent}`);
}
