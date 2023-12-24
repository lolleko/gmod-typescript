export function transformDefault(value: string | undefined) {
    if (value) {
        // Remove leading zeros in the front
        if (/^00\d+$/.test(value)) return value.replace(/^0+/, '');
    }
    return value;
}

export function transformType(type: string) {
    if (type === 'vararg') {
        return 'any[]';
    }
    return type
        .replace('table', 'any')
        .replace('function', 'Function')
        .replace(' or ', ' | ')
        .replace(/(\w) (\w)/g, '$1_$2');
}

export function transformIdentifier(id: string) {
    if (id == 'constructor') {
        return 'constructor';
    }
    const invalidIDMap: Record<string, string> = {
        class: 'class_',
        function: 'function_',
        var: 'var_',
        default: 'default_',
        new: 'new_',
        delete: 'delete_',
    };

    if (invalidIDMap[id]) {
        return invalidIDMap[id];
    }
    if (id === '') {
        return 'MISSING_WIKI_DATA';
    }
    if (id === '...') {
        return 'vararg';
    }

    return (
        id
            .replace(/\./g, '')
            // https://wiki.facepunch.com/gmod/Structures/PropertyAdd StructureField (Order)
            .replace(/\(.*\)/g, '')
            .replace(/[\/ ]/g, '_')
    );
}
