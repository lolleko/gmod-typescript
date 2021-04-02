import {
    isModifiyArgumentModification,
    isModifyReturnModification,
    getPageMods,
    isRenameIndentifierModification,
} from "./modification_db";
import { TSArgument, TSFunction, TSReturn } from "../ts_types";
import { WikiFunction } from "../wiki_types";
import { transformDescription } from "./description";
import { transformIdentifier, transformType } from "./util";

export function transformFunction(func: WikiFunction): TSFunction {
    const args: TSArgument[] = transformArgs(func);

    const ret = transformReturns(func);

    const docComment =
        transformDescription(func.description) +
        "\n" +
        func.args
            .map(
                (a) =>
                    `@param ${transformIdentifier(a.name)} - ${transformDescription(
                        a.description
                    ).replace(/\n{2,}/g, "\n")}`
            )
            .join("\n");

    return {
        identifier: func.name,
        args,
        docComment, // TODO extract from params aswell and parse inline tags
        ret,
    };
}

function transformArgs(func: WikiFunction): TSArgument[] {
    const mods = getPageMods(func.address);
    const argMods = mods.filter(isModifiyArgumentModification);

    return func.args.map((arg) => {
        let type = inferType(arg.type, arg.description);

        const argMod = argMods.find((a) => a.arg.identifier === arg.name);

        let defaultValue = arg.default;

        if (argMod) {
            if (argMod.arg.type) {
                type = argMod.arg.type;
            }
            if (argMod.arg.default) {
                defaultValue = argMod.arg.default;
            }
        }

        return {
            identifier: transformIdentifier(arg.name),
            default: defaultValue,
            type: transformType(type),
        } as TSArgument;
    });
}

function inferType(type: string, desc: string) {
    const links = desc.match(/<page>(.*?)<\/page>/);
    if (links) {
        const mods = getPageMods(links[1]);
        const renameMods = mods.filter(isRenameIndentifierModification);
        if (renameMods.length > 0) {
            type = renameMods[0].newName;
        } else if (links[1].includes("Structures")) {
            type = links[1].split("/")[1];
        } else if (links[1].includes("Enum")) {
            type = links[1].split("/")[1];
        } else if (links[1] === "Color") {
            type = "Color";
        }
    }
    return type;
}

function transformReturns(func: WikiFunction): TSReturn {
    const rets = func.rets;

    const mods = getPageMods(func.address);
    const retMod = mods.find(isModifyReturnModification);

    if (retMod) {
        return { type: retMod.return.type };
    }

    if (rets.length === 0) {
        return { type: "void" };
    }
    if (rets.length === 1) {
        return { type: transformType(inferType(rets[0].type, rets[0].description)) };
    }
    return {
        type: `LuaMultiReturn<[${rets
            .map((r) => transformType(inferType(r.type, r.description)))
            .join(", ")}]>`,
    };
}
