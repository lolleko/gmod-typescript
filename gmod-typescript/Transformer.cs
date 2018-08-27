using System;
using System.Collections.Generic;

namespace gmod_typescript
{
    public abstract class Transformer
    {
        private bool Parallel;

        protected Transformer(bool parallel) {
            Parallel = parallel;
        }

        public JsonType.WikiData Transform(JsonType.WikiData data) {
            var clone = CloneWikiData(data);
            TransformEnums(clone.Enums);
            TransformStructures(clone.Structures);
            TransformFunctionCollections(clone.FunctionCollections);

            return clone;
        }

        private JsonType.WikiData CloneWikiData(JsonType.WikiData data) {
            return JsonType.WikiData.FromJson(JsonType.Serialize.ToJson(data));
        }

        protected abstract void TransformEnums(List<JsonType.Enum> enums);

        protected abstract void TransformStructures(List<JsonType.Structure> structures);

        protected abstract void TransformFunctionCollections(List<JsonType.FunctionCollection> functionCollections);

        protected void ApplyActionIfPredicate<T>(List<T> list, Predicate<T> predicate, Action<T> modifier) {
            list.ForEach(item =>
            {
                if (predicate(item))
                {
                    modifier(item);
                }
            });
        }
    }
}
