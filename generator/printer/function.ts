import { TSArgument, TSFunction } from '../ts_types';
import { printDocComent } from './util';

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
    // try to infer default

    const mapArg = (a: TSArgument) => {
        const isOptional = a.default != undefined;

        // if the default is a simple bool or number we can use it in TS
        // if it is more complex we cant.
        // maybe add support for some common defaults in the future
        // TODO move this to the transformer
        const canParseDefault =
            a.default != undefined
                ? a.default.trim() == 'false' ||
                  a.default.trim() == 'true' ||
                  (!isNaN(a.default.trim() as any) && !isNaN(parseFloat(a.default.trim())))
                : false;

        if (canParseDefault) {
            return `${a.identifier} = ${a.default}`;
        } else if (isOptional) {
            return `${a.identifier}?: ${a.type}`;
        } else {
            return `${a.identifier}: ${a.type}`;
        }
    };

    const args = func.args.map(mapArg).join(', ');

    const docComment = printDocComent(func.docComment);

    const prefix = `${prependDeclare ? 'declare ' : ''}${prependFunction ? 'function ' : ''}`;

    return `
${docComment}
${prefix}${func.identifier}(${args}): ${func.ret.type};
`.trim();
}
