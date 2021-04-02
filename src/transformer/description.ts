import { parseMarkup } from "../scrapper/util";

export function transformDescription(description: string): string {
    if (description === "") {
        return "";
    }

    const descriptionEscaped = description.replace(/\/\*/g, "/").replace(/\*\//g, "\\");

    const descriptionWithMarkdownLinks = descriptionEscaped.replace(
        /<page>(.*?)<\/page>/g,
        "[$1](https://wiki.facepunch.com/gmod/$1)"
    );

    const descriptionObj = parseMarkup(descriptionWithMarkdownLinks);

    const bugToString = (bug: any) =>
        `**Bug ${
            bug.attr && bug.attr.issue
                ? `[#${bug.attr.issue}](https://github.com/Facepunch/garrysmod-issues/issues/${bug.attr.issue})`
                : ""
        }:**\n>${bug.__text ?? bug}\n`;

    const bugs: string = descriptionObj.bug
        ? Array.isArray(descriptionObj.bug)
            ? descriptionObj.bug.map(bugToString).join("\n")
            : bugToString(descriptionObj.bug)
        : "";

    const noteToString = (note: any) => `**Note:**\n>${note.__text ?? note}\n`;

    const notes: string = descriptionObj.note
        ? Array.isArray(descriptionObj.note)
            ? descriptionObj.note.map(noteToString).join("\n")
            : noteToString(descriptionObj.note)
        : "";

    const warningToString = (warning: any) => `**Warning:**\n>${warning.__text ?? warning}\n`;

    const warnings: string = descriptionObj.warning
        ? Array.isArray(descriptionObj.warning)
            ? descriptionObj.warning.map(warningToString).join("\n")
            : warningToString(descriptionObj.warning)
        : "";

    const deprecatedToString = (deprecated: any) =>
        `@deprecated ${deprecated.__text ?? deprecated}\n`;

    const deprecated: string = descriptionObj.deprecated
        ? Array.isArray(descriptionObj.deprecated)
            ? descriptionObj.deprecated.map(deprecatedToString).join("\n")
            : deprecatedToString(descriptionObj.deprecated)
        : "";

    const descriptionWithoutInlineTags = descriptionWithMarkdownLinks
        .replace(/<((bug)|(note)|(warning)|(deprecated))(.*?)>(.|\n)*<\/((bug)|(note)|(warning)|(deprecated))>/g, "")
        .replace(/<internal><\/internal>/g, "")
        .replace(/<internal>(.|\n)*<\/internal>/g, "**$1**")
        .trim();

    const append = (s: string) => (s === "" ? "" : `\n\n${s}`);

    const result = `${descriptionWithoutInlineTags}${append(deprecated)}${append(warnings)}${append(
        bugs
    )}${append(notes)}`;

    const unescapedResult = result
        .replace(/&amp;/g, "&")
        .replace(/&lt;/g, "<")
        .replace(/&gt;/g, ">")
        .replace(/&grave;/g, "`");

    return unescapedResult;
}
