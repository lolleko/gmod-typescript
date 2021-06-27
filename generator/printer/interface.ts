import { TSCollection, TSField } from '../ts_types';
import { printInterfaceFunction, printNamespaceFunction } from './function';
import { indentStr, printDocComent } from './util';

export function printInterface(tsInterface: TSCollection): string {
    return _printInterface(tsInterface);
}

export function _printInterface(
    tsInterface: TSCollection,
    indent = '    ',
    innerCollection = false
): string {
    let head: string;
    let functions: string;
    let fields: string;
    let docComment: string = printDocComent(tsInterface.docComment);
    if (tsInterface.namespace) {
        head = `${!innerCollection ? 'declare ' : ''}namespace ${tsInterface.identifier} {`;
        head = innerCollection ? indentStr(head, '    ') : head;
        docComment = innerCollection ? indentStr(docComment, '    ') : docComment;
        functions = indentStr(
            tsInterface.functions.map(printNamespaceFunction).join('\n\n'),
            indent
        );
        fields = indentStr(
            tsInterface.fields.map((f) => printInterfaceField(f, true)).join('\n\n'),
            indent
        );
    } else {
        const parent = tsInterface.parent ? `extends ${tsInterface.parent} ` : '';
        head = `interface ${tsInterface.identifier} ${parent}{`;
        functions = indentStr(
            tsInterface.functions.map(printInterfaceFunction).join('\n\n'),
            indent
        );
        fields = indentStr(
            tsInterface.fields.map((f) => printInterfaceField(f)).join('\n\n'),
            indent
        );
    }

    return `
${docComment}
${head}
${fields}

${functions}
${tsInterface.innerCollections.map((ic) => _printInterface(ic, indent + indent, true)).join('\n\n')}
${innerCollection ? indentStr('}', '    ') : '}'}
`.trim();
}

export function printInterfaceField(tsInterfaceField: TSField, isNamespace = false) {
    return `
${printDocComent(tsInterfaceField.docComment)}
${isNamespace ? 'const ' : ''}${tsInterfaceField.identifier}${
        tsInterfaceField.optional ? '?' : ''
    }: ${tsInterfaceField.type}${isNamespace ? ';' : ','}
`.trim();
}
