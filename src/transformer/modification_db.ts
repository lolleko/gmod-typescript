import { TSArgument, TSFunction, TSReturn } from "../ts_types";

import * as modificationDB from "./modifications.json";

export function getPageMods(page: string): Modification[] {
    if ((modificationDB as ModificationDB)[page]) {
        return (modificationDB as ModificationDB)[page];
    }
    return [];
}

export interface ModificationDB extends Record<string, Modification[]> {}

export enum ModificationKind {
    ModifiyArgument = "ModifiyArgument",
    ModifyReturn = "ModifyReturn",
    OmitParentField = "OmitParentField",
    RenameIndentifier = "RenameIndentifier",
    InnerNamespace = "InnerNamespace",
}

export interface Modification {
    kind: ModificationKind;
}

export interface ModifiyArgumentModification extends Modification {
    kind: ModificationKind.ModifiyArgument;
    arg: Partial<TSArgument>;
}

export function isModifiyArgumentModification(
    mod: Modification
): mod is ModifiyArgumentModification {
    return mod.kind === ModificationKind.ModifiyArgument;
}

export interface ModifyReturnModification extends Modification {
    kind: ModificationKind.ModifyReturn;
    return: TSReturn;
}

export function isModifyReturnModification(mod: Modification): mod is ModifyReturnModification {
    return mod.kind === ModificationKind.ModifyReturn;
}

export interface OmitParentFieldModification extends Modification {
    kind: ModificationKind.OmitParentField;
    omit: string;
}

export function isOmitParentFieldModification(
    mod: Modification
): mod is OmitParentFieldModification {
    return mod.kind === ModificationKind.OmitParentField;
}

export interface RenameIndentifierModification extends Modification {
    kind: ModificationKind.RenameIndentifier;
    newName: string;
    fieldNale?: string;
}

export function isRenameIndentifierModification(
    mod: Modification
): mod is RenameIndentifierModification {
    return mod.kind === ModificationKind.RenameIndentifier;
}

export interface InnerNamespaceModification extends Modification {
    kind: ModificationKind.InnerNamespace;
    prefix: string;
}

export function isInnerNamespaceModification(mod: Modification): mod is InnerNamespaceModification {
    return mod.kind === ModificationKind.InnerNamespace;
}
