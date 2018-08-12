using System;
using System.Collections.Generic;
using System.Linq;

namespace gmod_typescript
{
    public class ClassAndHooksFunctionsCategory : WikiObject
    {
        public override string ToString()
        {
            // Hooks
            var extendsMap = new Dictionary<string, string>
            {
                { "NEXTBOT", "ENTITY" },
                { "PLAYER", "ENTITY" },
                { "WEAPON", "ENTITY" },
            };

            var hookFuncs = GetGmodFunctionsByCategory("Hooks");
            var hookList = hookFuncs
                .AsParallel()
                .Select(funcGroup =>
                {
                    string cat = funcGroup.Key;
                    string extends = "";
                    if (extendsMap.ContainsKey(cat))
                    {
                        extends = extendsMap[cat];
                    }
                    return new FunctionCategoryArticle("Category:" + cat + "_Hooks", funcGroup.AsEnumerable(), CategoryType.Class, extends);
                })
                .ToList();

            // Classes
            var classFuncs = GetGmodFunctionsByCategory("Class_Functions");
            var classList = classFuncs
                .AsParallel()
                .Select(funcGroup => new FunctionCategoryArticle("Category:" + funcGroup.Key, funcGroup.AsEnumerable(), CategoryType.Interface))
                .ToList();


            // Extend Hooks with class functions
            var extractHookClasses = new Dictionary<string, string>
            {
                { "Category:Entity", "Category:ENTITY_Hooks" },
                { "Category:NPC", "Category:ENTITY_Hooks" },
                { "Category:Weapon", "Category:WEAPON_Hooks" },
                { "Category:Tool", "Category:TOOL_Hooks" },
                { "Category:NextBot", "Category:NEXTBOT_Hooks" }
            };

            var x = classList.FindAll(cl => extractHookClasses.ContainsKey(cl.Title));

            classList.FindAll(cl => extractHookClasses.ContainsKey(cl.Title))
                     .ForEach(cl => {
                             var hook = hookList.Find(h => h.Title == extractHookClasses[cl.Title]);
                             // TODO maybe fix in Wiki instead
                             if (cl.Title == "Category:Entity")
                             {
                                 cl.WikiFunctions.RemoveAll(c => c.Title == "Use");
                             }
                             hook.WikiFunctions.AddRange(cl.WikiFunctions);
                         });

            classList.RemoveAll(cl => extractHookClasses.ContainsKey(cl.Title));

            return string.Join("\n", hookList) + string.Join("\n", classList);
        }
    }
}
