## Typescript Declarations for Gmod

### What's new

- Updated to use new Facepunch wiki as source
- Updated to latest tstl version (requires 0.38.0 or higher)

### TODO

- Create npm package
- create template/example project

### Usage

1. Drop the files from `out` into your tyeposcript project
2. modify the tsconfig, add the declaration files to your `types` or `typeRoots`

### Development

1. Clone
2. `npm install`
3. `npm run build` to build the project
4. `npm run generate` to generate declarations

#### Program structure

- Scrapper
    - Fetching wiki data
    - Conversion of wiki xml to [Wiki Objects](./src/wiki_types.ts)
- Transformer
    - Transforms extracted wiki objects to [Typed Objects](./src/ts_types.ts)
- Printer
    - Print the typed objects to `out/declarations.ts`
