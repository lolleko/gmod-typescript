/** #TopLevel */
class WikiData {
    /**
     * A dictionary of Contacts, indexed by unique ID
     */
    functionCollections: FunctionCollection[];
    structures: Structure[];
    enums: Enum[];
  }
  
  class FunctionCollection {
    name: string;
    description: string;
    collectionType: "class" | "library" | "global";
    extends: string;
    customConstructor: string;
    examples: Example[];
    isHook: boolean;
    isPureAbstract: boolean;
    classFields: Field[];
    functions: Function_[];
  }
  
  class Field {
      name: string;
      type: string;
      description: string;
      default: string;
      isOptional: boolean;
  }
  
  class Function_ {
      name: string;
      description: string;
      examples: Example[];
      realm: "server" | "client" | "menu" | "shared" | "shared and menu" | "client and menu";
      returns: Return[];
      arguments: Argument[];
      isConstructor: boolean;
      accessModifier: "private" | "protected" | "public";
  }

  class Example {
      description: string;
      code: string;
  } 
  
  class Return {
      type: string;
      description: string;
  }
  
  class Argument {
      name: string;
      description: string;
      type: string;
      default: string;
      isVarArg: boolean;
      isOptional: boolean;
  }
  
  class Structure {
      name: string;
      description: string;
      structureFields: Field[];
  }
  
  class Enum {
      name: string;
      description: string;
      isMembersOnly: boolean;
      enumFields: EnumField[];
  }
  
  class EnumField {
      name: string;
      description: string;
      /** @TJS-type integer */
      value: number;
  }
  