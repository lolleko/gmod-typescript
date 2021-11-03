export interface TSDocumented {
    docComment: string;
}

export interface TSCollection extends TSDocumented {
    identifier: string;
    fields: TSField[];
    functions: TSFunction[];
    innerCollections: TSCollection[];
    parent?: string;
    namespace: boolean;
}

export interface TSNamespace extends TSCollection {}

export interface TSField extends TSDocumented {
    identifier: string;
    type: string;
    optional: boolean;
}

export interface TSFunction extends TSDocumented {
    identifier: string;
    args: TSArgument[];
    ret: TSReturn;
}

export interface TSArgument {
    identifier: string;
    type: string;
    default?: string;
}

export interface TSReturn {
    type: string;
}

export interface TSEnum extends TSDocumented {
    identifier: string;
    fields: TSEnumField[];
    compileMembersOnly: boolean;
}

export interface TSEnumField extends TSDocumented {
    identifier: string;
    value: string;
}
