using System;
using System.Linq;

namespace gmod_typescript
{
    public abstract class Serializer
    {
        public Serializer()
        {
        }

        public string Serialize(JsonType.WikiData data, Transformer transformer = null)
        {
            if (transformer != null) {
                data = transformer.Transform(data);
            }

            string result = "";
            data.Enums.Sort((x, y) => string.Compare(x.Name, y.Name));
            var enumStrList = data.Enums.Select(SerializeEnum);
            result += string.Join("\n", enumStrList);

            data.Structures.Sort((x, y) => string.Compare(x.Name, y.Name));
            var structStrList = data.Structures.Select(SerializeStructure);
            result += string.Join("\n", structStrList);

            data.FunctionCollections.Sort((x, y) => string.Compare(x.Name, y.Name));
            data.FunctionCollections.ForEach(
                funcColl => funcColl.Functions.Sort(
                    (x, y) =>
                    {
                        if (x.IsConstructor) {
                            return -1;
                        }
                        if (y.IsConstructor) {
                            return 1;
                        }
                        return string.Compare(x.Name, y.Name);
                    }));
            var fucntionCollectionList = data.FunctionCollections.Select(SerializeFunctionCollection);
            result += string.Join("\n", fucntionCollectionList);

            return result;
        }

        public abstract string SerializeFunctionCollection(JsonType.FunctionCollection functionCollection);
        public abstract string SerializeFunction(JsonType.Function function);
        public abstract string SerializeArgument(JsonType.Argument argument);
        public abstract string SerializeReturn(JsonType.Return _return);
        public abstract string SerializeField(JsonType.Field field);
        public abstract string SerializeEnum(JsonType.Enum _enum);
        public abstract string SerializeEnumField(JsonType.EnumField enumField);
        public abstract string SerializeExample(JsonType.Example example);
        public abstract string SerializeStructure(JsonType.Structure structure);
    }
}
