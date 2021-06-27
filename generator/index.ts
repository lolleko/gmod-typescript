import { printGlobalFunction } from './printer/function';
import { extractEnum } from './scrapper/extract/enum';
import { extractFunction } from './scrapper/extract/function';
import { extractStruct } from './scrapper/extract/struct';
import { GetPage, GetPagesInCategory } from './scrapper/scrapper';
import { transformFunction } from './transformer/function';
import * as fs from 'fs';
import { transformEnum } from './transformer/enum';
import { printEnum } from './printer/enum';
import { transformStruct } from './transformer/struct';
import { printInterface } from './printer/interface';
import { transformFunctionCollection } from './transformer/function_collection';
import { extractClass } from './scrapper/extract/class';
import { extractLibrary } from './scrapper/extract/library';
import { isWikiFunction } from './wiki_types';

(async (): Promise<void> => {
    const globalFuncs = await GetPagesInCategory('Global');
    const globalFunctionPages = await Promise.all(globalFuncs.map((page) => GetPage(page)));
    const globalFunctionResult = globalFunctionPages
        .map(extractFunction)
        .filter(isWikiFunction)
        .map(transformFunction)
        .map(printGlobalFunction)
        .join('\n\n');

    const enums = await GetPagesInCategory('enum');
    const enumPages = await Promise.all(enums.map((page) => GetPage(page)));
    const enumResult = enumPages.map(extractEnum).map(transformEnum).map(printEnum).join('\n\n');

    const structs = await GetPagesInCategory('struct');
    const structPages = await Promise.all(structs.map((page) => GetPage(page)));
    const structResult = structPages
        .map(extractStruct)
        .map(transformStruct)
        .map(printInterface)
        .join('\n\n');

    const classFuncPaths = [
        ...(await GetPagesInCategory('classfunc')),
        ...(await GetPagesInCategory('panelfunc')),
    ];
    const classFuncsPages = await Promise.all(classFuncPaths.map((page) => GetPage(page)));
    const classFuncs = classFuncsPages.filter((p) => p.title.includes(':')).map(extractFunction);
    const classes = classFuncsPages.filter((p) => !p.title.includes(':')).map(extractClass);

    const classResult = classes
        .map((wikiClass) =>
            transformFunctionCollection(
                wikiClass,
                classFuncs.filter((cf) => cf.parent === wikiClass.name)
            )
        )
        .map(printInterface)
        .join('\n\n');

    const libraryFuncPaths = await GetPagesInCategory('libraryfunc');
    const libraryFuncPages = await Promise.all(libraryFuncPaths.map((page) => GetPage(page)));
    const libraryFuncs = libraryFuncPages.filter((p) => p.title.includes('.')).map(extractFunction);
    const libraries = libraryFuncPages.filter((p) => !p.title.includes('.')).map(extractLibrary);

    const libraryResult = libraries
        .map((wikiLibrary) =>
            transformFunctionCollection(
                wikiLibrary,
                libraryFuncs.filter((cf) => cf.parent === wikiLibrary.name)
            )
        )
        .map(printInterface)
        .join('\n\n');

    const result = [
        '/// <reference types="typescript-to-lua/language-extensions" />',
        '/// <reference path="./extras.d.ts" />',
        '/** @noSelfInFile **/',
        classResult,
        structResult,
        enumResult,
        globalFunctionResult,
        libraryResult,
    ].join('\n\n');

    fs.writeFileSync('types/generated.d.ts', result);
})();
