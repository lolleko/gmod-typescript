using System;
using System.Collections.Generic;
using System.Linq;

namespace gmod_typescript
{
    public class TransformerTypescript : Transformer
    {
        public TransformerTypescript(bool parallel) : base(parallel)
        {

        }

        protected override void TransformEnums(List<JsonType.Enum> enums)
        {
            enums.RemoveAll(e => e.Name == "STENCIL");

            enums.Find(e => e.Name == "PLAYER").Name = "PLAYER_ANIM";
        }

        protected override void TransformFunctionCollections(List<JsonType.FunctionCollection> functionCollections)
        {
            // clear everything with a dot
            functionCollections.RemoveAll(fc => fc.Name.Contains('.'));
            ApplyActionIfPredicate(functionCollections,
                                   fc => true,
                                   fc => fc.Functions.RemoveAll(f => f.Name.Contains('.')));
            // function filter
            var functionDict = new Dictionary<string, string>{
                {"Entity", "Use"},
                {"Global", "Error"}
            };
            ApplyActionIfPredicate(functionCollections,
                                   fc => functionDict.ContainsKey(fc.Name),
                                   fc => fc.Functions.RemoveAll(f => f.Name == functionDict[fc.Name]));

            var changeReturnTypeDict = new Dictionary<string, List<(string, string)>>
            {
                { "ents", new List<(string,string)>{
                        ("FindAlongRay", "Entity[]"),
                        ("FindByClass", "Entity[]"),
                        ("FindByClassAndParent", "Entity[]"),
                        ("FindByModel", "Entity[]"),
                        ("FindByName", "Entity[]"),
                        ("FindInBox", "Entity[]"),
                        ("FindInCone", "Entity[]"),
                        ("FindInSphere", "Entity[]"),
                        ("FindInCone", "Entity[]"), } },
                { "player", new List<(string,string)>{
                        ("GetBots", "Player[]"),
                        ("GetHumans", "Player[]")}},
                { "WEAPON", new List<(string,string)>{
                        ("Think", "boolean")}},
                { "file", new List<(string,string)>{
                        ("Find", "string[], string[]")}}
            };
            ApplyActionIfPredicate(functionCollections,
                                   fc => changeReturnTypeDict.ContainsKey(fc.Name),
                                   fc => fc.Functions.ForEach(f =>
                                   {
                                       var funcList = changeReturnTypeDict[fc.Name];
                                       var newRetType = funcList.Find((funcTuple) => funcTuple.Item1 == f.Name).Item2;
                                       if (newRetType != null)
                                       {
                                           if (f.Returns.Count > 0)
                                           {
                                               var multRets = newRetType.Split(",").ToList();
                                               int i = 0;
                                               f.Returns.ForEach(ret =>
                                               {
                                                   if (i < multRets.Count)
                                                   {
                                                       ret.Type = multRets[i];
                                                   }
                                                   i++;
                                               });
                                           }
                                           else
                                           {
                                               f.Returns.Add(new JsonType.Return { Type = newRetType, Description = "" });
                                           }
                                       }
                                   }));


            var changeArgumentTypeDict = new Dictionary<string, List<(string, List<string>)>>
            {

                { "Entity", new List<(string,List<string>)>{
                        ("SetSequence", new List<string>{"number | string"}),
                        ("ResetSequence", new List<string>{"number | string"})}},
            };
            ApplyActionIfPredicate(functionCollections,
                                   fc => changeArgumentTypeDict.ContainsKey(fc.Name),
                                   fc => fc.Functions.ForEach(f =>
                                   {
                                       var funcList = changeArgumentTypeDict[fc.Name];
                                       var newArgTypes = funcList.Find((funcTuple) => funcTuple.Item1 == f.Name).Item2;
                                       if (newArgTypes != null)
                                       {
                                           for (int i = 0; i < newArgTypes.Count; i++)
                                           {
                                               f.Arguments[i].Type = newArgTypes[i];
                                           }
                                       }
                                   }));

            // Add some convenience hook.Add definitions
            var hooks = new List<JsonType.FunctionCollection> { functionCollections.Find(fc => fc.Name == "GM"), functionCollections.Find(fc => fc.Name == "SANDBOX") };
            var hookLib = functionCollections.Find(fc => fc.Name == "hook");
            var originalHookAdd = hookLib.Functions.Find(fa => fa.Name == "Add");

            foreach (var hookCat in hooks)
            {
                foreach (var hookFunc in hookCat.Functions)
                {
                    var extraHookAdd = JsonType.Extension.Clone(originalHookAdd);
                    extraHookAdd.Arguments[0].Type = "\"" + hookFunc.Name + "\"";

                    var returns = "void";
                    if (hookFunc.Returns.Count >= 1)
                    {
                        returns += $" | {hookFunc.Returns[0].Type}";
                    }

                    var serializer = new SerializerTypescript();
                    extraHookAdd.Arguments[2].Type = $"({string.Join(", ", hookFunc.Arguments.Select(serializer.SerializeArgument))}) => {returns}";
                    extraHookAdd.Arguments[2].Description = "see { @link " + hookCat.Name + "#" + hookFunc.Name + "}";
                    hookLib.Functions.Add(extraHookAdd);
                }
            }


            foreach (var funcColl in functionCollections)
            {
                JsonType.Function additionalFunc = null;
                foreach (var func in funcColl.Functions)
                {
                    // optionals fix
                    if (func.Arguments.Count > 0)
                    {
                        var firstOptional = func.Arguments.SkipWhile(a => !a.IsOptional);
                        bool removeOptionals = !firstOptional.All(a => a.IsOptional);

                        if (removeOptionals)
                        {
                            additionalFunc = JsonType.Extension.Clone(func);
                            additionalFunc.Arguments = additionalFunc.Arguments.Where(a => !a.IsOptional).ToList();

                            func.Arguments = func.Arguments.TakeWhile(a => !a.IsOptional)
                                .Concat(firstOptional.Select(a =>
                                {
                                    if (!a.IsVarArg)
                                    {
                                        a.IsOptional = true;
                                    }
                                    return a;
                                })).ToList();
                        }
                    }
                }
                if (additionalFunc != null)
                {
                    funcColl.Functions.Add(additionalFunc);
                }
            }

            // Resolve subclass overloads
            foreach (var funcColl in functionCollections)
            {
                if (funcColl.Extends != "")
                {
                    var overridenFuncs = new List<JsonType.Function>();
                    for (int i = 0; i < funcColl.Functions.Count; i++)
                    {
                        var wikiFunc = funcColl.Functions[i];
                        var extends = funcColl.Extends;
                        while (extends != "")
                        {
                            var baseClass = functionCollections.Find(fc => fc.Name == extends);
                            var baseClassFunc = baseClass.Functions.Find(wc => wc.Name == wikiFunc.Name);
                            if (baseClassFunc != null)
                            {
                                overridenFuncs.Add(baseClassFunc);
                            }
                            extends = baseClass.Extends;
                        }
                    }
                    overridenFuncs.ForEach(func => funcColl.Functions.Add(func));
                }
            }

            // merge
            var mergeDict = new Dictionary<string, string>
            {
                { "ENTITY", "Entity" },
                { "NEXTBOT", "NextBot" },
                { "WEAPON", "Weapon" },
                { "PANEL", "Panel"}
            };

            ApplyActionIfPredicate(functionCollections,
                                   fc => mergeDict.ContainsKey(fc.Name),
                                   source =>
                                   {
                                       var target = functionCollections.Find(fc => fc.Name == mergeDict[source.Name]);
                                       // Dont override so hooks that can be called publically are still avaialble
                                       var noDuplicates = source.Functions.Where(wf => target.Functions.Find(tf => tf.Name == wf.Name) == null);
                                       //.Select(func => { func.AccessModifier = JsonType.AccessModifier.Protected; return func; });
                                       target.Functions.AddRange(noDuplicates);
                                   });

            functionCollections.RemoveAll(fc => mergeDict.ContainsKey(fc.Name));

            // Extend
            var extendsMap = new Dictionary<string, string>
            {
                { "SANDBOX", "Gamemode" },
                { "NextBot", "Entity" },
                { "Player", "Entity" },
                { "Weapon", "Entity" },
                { "NPC", "Entity" },
                { "Vehicle", "Entity" },
                { "CSEnt", "Entity" },
            };
            ApplyActionIfPredicate(functionCollections,
                                   fc => extendsMap.ContainsKey(fc.Name),
                                   fc => fc.Extends = extendsMap[fc.Name]);

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

            ApplyActionIfPredicate(functionCollections,
                                   fc => customConstructors.ContainsKey(fc.Name),
                                   fc =>
                                   {
                                       var constructorName = customConstructors[fc.Name];
                                       var globalCat = functionCollections.Find(c => c.Name == "Global");
                                       var customConstructor = globalCat.Functions.Find(wf => wf.Name == constructorName);
                                       customConstructor.Name = "constructor";
                                       customConstructor.Returns = new List<JsonType.Return>();
                                       customConstructor.IsConstructor = true;
                                       fc.Functions.Insert(0, customConstructor);
                                       fc.CustomConstructor = constructorName;
                                       globalCat.Functions.Remove(customConstructor);
                                   });


            var renameDict = new Dictionary<string, string>
            {
                {"GM", "Gamemode"}
            };

            ApplyActionIfPredicate(functionCollections,
                                   fc => renameDict.ContainsKey(fc.Name),
                                   fc =>
                                   {
                                       fc.Name = renameDict[fc.Name];
                                   });
        }

        protected override void TransformStructures(List<JsonType.Structure> structures)
        {
            // rename
            var renameDict = new Dictionary<string, string>{
                {"ENT", "Entity"},
                {"SWEP", "Weapon"},
                {"GM", "Gamemode"}
            };
            ApplyActionIfPredicate(structures,
                                   s => renameDict.ContainsKey(s.Name),
                                   s => s.Name = renameDict[s.Name]);
            // SWEP is used as standalone table in some places so re-add it
            var weaponStruct = structures.Find(s => s.Name == "Weapon");
            var swepStruct = JsonType.Extension.Clone(weaponStruct);
            swepStruct.Name = "SWEP";
            structures.Add(swepStruct);

            // ENT is used as standalone aswell
            var entityStruct = structures.Find(s => s.Name == "Entity");
            var entStruct = JsonType.Extension.Clone(entityStruct);
            entStruct.Name = "ENT";
            structures.Add(entStruct);
        }
    }
}
