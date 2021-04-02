import { TSFunction } from "../ts_types";
import { printDocComent } from "./util";

export function printNamespaceFunction(func: TSFunction): string {
    return printFunction(func, true, false);
}

export function printInterfaceFunction(func: TSFunction): string {
    return printFunction(func, false, false);
}

export function printGlobalFunction(func: TSFunction): string {
    return printFunction(func, true, true);
}

export function printFunction(
    func: TSFunction,
    prependFunction: boolean,
    prependDeclare: boolean
): string {
    const args = func.args.map((a) => `${a.identifier}: ${a.type}`).join(", ");

    const docComment = printDocComent(func.docComment);

    const prefix = `${prependDeclare ? "declare " : ""}${prependFunction ? "function " : ""}`;

    return `
${docComment}
${prefix}${func.identifier}(${args}): ${func.ret.type};
`.trim();
}
