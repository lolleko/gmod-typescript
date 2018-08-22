using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gmod_typescript
{
    public class FunctionCollection : WikiObject
    {
        private Dictionary<string, FunctionCategoryArticle> functionList;

        private List<EnumerationArticle> enumList;

        private List<StructureArticle> structList;

        public FunctionCollection()
        {
            functionList = new Dictionary<string, FunctionCategoryArticle>();
            enumList = new List<EnumerationArticle>();
            structList = new List<StructureArticle>();
        }

        public void AddFunctionCategory(string category, FunctionCategoryArticle article) {
            category = category.Replace("Category:", "");
            if (!functionList.ContainsKey(category)) {
                functionList.Add(category, article);
            }
        }

        public async Task AddFunctionsFromCategory(string category, CategoryType type)
        {
            await Task.Run(() =>
            {
                foreach(var funcGroup in GetGmodFunctionsByCategory(category))
                {
                    AddFunctionCategory(funcGroup.Key, new FunctionCategoryArticle(funcGroup, type));
                }
            });
        }

        public async Task AddEnumerations() {
            await Task.Run(() =>
            {
                foreach (var enumeration in GetPagesInCategory("Enumerations"))
                {
                    enumList.Add(new EnumerationArticle(enumeration));
                }
            });
        }

        public async Task AddStructures()
        {
            await Task.Run(() =>
            {
                foreach (var structure in GetPagesInCategory("Structures"))
                {
                    structList.Add(new StructureArticle(structure));
                }
            });
        }

        public async Task AddPanels()
        {
            await Task.Run(() =>
            {
                foreach (var panel in GetPagesInCategory("Panels"))
                {
                    AddFunctionCategory(panel, new FunctionCategoryArticle(panel, GetPagesInCategory(panel.Replace("Category:", "")), CategoryType.Class));
                }
            });
        }

        public string EnumsToString() {
            var escapedEnums = enumList.Where(e => e.Title != "STENCIL");
            return string.Join("\n", escapedEnums);
        }

        public string StructuresToString()
        {
            var renamedStructList = structList.Select(structure =>
            {
                switch (structure.Name)
                {
                    case "ENT":
                        structure.Name = "Entity";
                        break;
                    case "SWEP":
                        structure.Name = "Weapon";
                        break;
                }
                return structure;
            }).ToList();
            // SWEP is used as standalone table in soem places so readd it
            renamedStructList.Add(new StructureArticle("Structures/SWEP"));
            return string.Join("\n", renamedStructList);
        }

        public string ClassesToString()
        {
            // TODO we loop for every moddifier right now this could be optimized

            // CategoryFilter
            var catFilter = new Dictionary<string, Predicate<FunctionCategoryArticle>>
            {
                { "*", fc => fc.CategoryTitle.Contains('.') }
            };

            ModifyFunctionsIfTitleContainedInDict(catFilter, (funcCat, filter) =>
            {
                if (filter(funcCat))
                {
                    functionList.Remove(funcCat.Url);
                }
            });

            // Funcition Filter
            var functionFilter = new Dictionary<string, Predicate<FunctionArticle>>
            {
                { "Entity", f => f.Title == "Use" },
                { "Global", f => f.Title == "Error" },
                { "*", f => f.Title.Contains('.') }
            };

            ModifyFunctionsIfTitleContainedInDict(functionFilter, (funcCat, filter) =>
            {
                funcCat.WikiFunctions.RemoveAll(filter);
            });


            // Add special
            // TODO this is a fucking mess
            var hooks = new List<FunctionCategoryArticle>{functionList["GM_Hooks"], functionList["SANDBOX_Hooks"]};
            var hookLib = functionList["hook"];
            var originalHookAdd = hookLib.WikiFunctions.Find(fa => fa.Title == "Add");

            foreach (var hookCat in hooks) {
                foreach (var hookFunc in hookCat.WikiFunctions) {
                    var returns = hookFunc.Returns;
                    // Always allow void return in hook callback
                    if (hookFunc.Returns != null && hookFunc.Returns.RetList != null)
                    {
                        var retList = hookFunc.Returns.RetList;
                        var modifiedReturn = new RetTemplate(retList[0].Raw, retList[0].Article);
                        if (modifiedReturn.TypeRaw != "void")
                        {
                            modifiedReturn.TypeRaw = modifiedReturn.TypeRaw + "| void";
                        }
                        returns = new Returns(new List<RetTemplate> { modifiedReturn });
                    }
                    var extraHookAdd = new FunctionArticle("hook/Add", true, false, originalHookAdd.Raw)
                    {
                        Function = originalHookAdd.Function,
                        Returns = originalHookAdd.Returns
                    };
                    extraHookAdd.Arguments = new Args(
                        new List<ArgTemplate>{
                        new ArgTemplate("{{Arg|name=eventName|type=\"" + hookFunc.Title + "\"|desc=The name of the GM Hook}}", extraHookAdd),
                            originalHookAdd.Arguments.Arguments[1],
                            new ArgTemplate("{{Arg|type=placeholder|name=func|desc=see {@link " + hookCat.CategoryTitle.Replace("_Hooks", "") + "#" + hookFunc.Title + "}}}", extraHookAdd)
                                {
                                    TypeRaw = "("+ hookFunc.Arguments +") => "+ returns
                                }
                        }
                    );
                    hookLib.WikiFunctions.Add(extraHookAdd);
                }
            }

            // Resolve subclass overloads
            foreach (var funcCat in functionList.Values) {
                if (funcCat.Extends != "") {
                    var overridenFuncs = new List<(int, FunctionArticle)>();
                    for (int i = 0; i < funcCat.WikiFunctions.Count(); i++)
                    {
                        var wikiFunc = funcCat.WikiFunctions[i];
                        var extends = funcCat.Extends;
                        while (extends != "") {
                            var baseClass = functionList[extends];
                            var baseClassFunc = baseClass.WikiFunctions.Find(wc => wc.Title == wikiFunc.Title);
                            if (baseClassFunc != null)
                            {
                                overridenFuncs.Add((i, baseClassFunc));
                            }
                            extends = baseClass.Extends;
                        }
                    }
                    for (int j = 0; j < overridenFuncs.Count(); j++)
                    {
                        var overridenFunc = overridenFuncs[j];
                        funcCat.WikiFunctions.Insert(overridenFunc.Item1 + j, overridenFunc.Item2);
                    }
                }
            }

            // Merge
            var mergeMap = new Dictionary<string, string>
            {
                { "ENTITY_Hooks", "Entity" },
                { "NEXTBOT_Hooks", "NextBot" },
                { "WEAPON_Hooks", "Weapon" }
            };

            ModifyFunctionsIfTitleContainedInDict(mergeMap, (source, targetTitle) =>
            {
                var target = functionList[targetTitle];
                // Dont override so hooks that can be called publically are still avaialble
                var noDuplicates = source.WikiFunctions.Where(wf => target.WikiFunctions.Find(tf => tf.Title == wf.Title) == null);
                target.WikiFunctions.AddRange(noDuplicates);
                functionList.Remove(source.CategoryTitle);
            });

            // Retype
            var retypeMap = new Dictionary<string, CategoryType>
            {
            };

            ModifyFunctionsIfTitleContainedInDict(retypeMap, (funcCat, newType) =>
            {
                funcCat.Type = newType;
            });

            // Extend
            var extendsMap = new Dictionary<string, string>
            {
                { "SANDBOX_Hooks", "GM" },
                { "NextBot", "Entity" },
                { "Player", "Entity" },
                { "Weapon", "Entity" },
                { "NPC", "Entity" },
                { "Vehicle", "Entity" },
                { "CSEnt", "Entity" },
            };
            ModifyFunctionsIfTitleContainedInDict(extendsMap, (funcCat, newParent) =>
            {
                funcCat.Extends = newParent;
            });

            // Create Custom Constructors
            var customConstructors = new Dictionary<string, string>
            {
                { "Entity", "Entity" },
                { "Player", "Player" },
                { "DLabel", "Label" },
                { "Vector", "Vector" },
                { "Angle", "Angle" },
                { "VMatrix", "Matrix" },
                { "IMesh", "Mesh" },
                { "IMaterial", "Material" },
                { "ProjectedTexture", "ProjectedTexture" },
                { "CDamageInfo", "DamageInfo" },
                { "CNewParticleEffect", "CreateParticleSystem" },
                { "PhysCollide", "CreatePhysCollideBox" },
                { "CSoundPatch", "CreateSound" },
                { "DSprite", "CreateSprite" }
            };

            ModifyFunctionsIfTitleContainedInDict(customConstructors, (funcCat, constructorName) =>
            {
                var globalCat = functionList["Global"];
                var customConstructor = globalCat.WikiFunctions.Find(
                    wf => wf.Title == constructorName);
                customConstructor.Name = "constructor";
                customConstructor.PrependDeclare = false;
                customConstructor.PrependFunc = false;
                customConstructor.Returns = null;
                funcCat.WikiFunctions.Insert(0, customConstructor);
                funcCat.CustomConstructor = constructorName;
                globalCat.WikiFunctions.Remove(customConstructor);
            });

            // Rename
            var renameMap = new Dictionary<string, string>
            {
            };

            ModifyFunctionsIfTitleContainedInDict(renameMap, (funcCat, newName) =>
            {
                funcCat.CategoryTitle = newName;
            });

            return string.Join("\n", functionList.Values);
        }

        public void ModifyFunctionsIfTitleContainedInDict<TValue>(Dictionary<string, TValue> dict, Action<FunctionCategoryArticle, TValue> modifier)
        {
            var valArr = functionList.Values.ToArray();
            for (int i = 0; i < valArr.Length; i++)
            {
                var funcCat = valArr[i];
                if (dict.ContainsKey(funcCat.CategoryTitle))
                {
                    modifier(funcCat, dict.GetValueOrDefault(funcCat.CategoryTitle));
                }
                if (dict.ContainsKey("*"))
                {
                    modifier(funcCat, dict.GetValueOrDefault("*"));
                }
            }
        }
    }
}
