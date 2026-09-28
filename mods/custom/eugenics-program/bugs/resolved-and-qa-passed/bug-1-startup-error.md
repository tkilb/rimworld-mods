# Bug Report: Startup XML Schema, Cross-Reference, and Texture Errors

**Status:** Passed QA
**Severity:** Critical

---

## 1. Environment & Target System

- **Machine:** Arch Linux Desktop / Steam Deck (SteamOS)
- **RimWorld Version:** 1.6
- **Active DLCs:** Royalty [ ] | Ideology [ ] | Biotech [x] | Anomaly [ ]
- **Relevant Active Mods:**
  - `brrainz.harmony`
  - `zal.cloning` (Biotech Cloning Continued)
  - `ninagoblin.enhancedvatlearning`
  - `Zaf.InjectGene`
  - `AmCh.Eragon.HCGeneFabrication`
  - `Defi.GeneRipper`
  - Other:

---

## 2. Affected Subsystem

Check the component(s) involved in the issue:

- [x] Initial Mod Loading
- [ ] **Gene Splicing UI (`Dialog_EditEmbryoGenes`):** Inspect gizmo, gene addition/removal, template matching, disc burning.
- [ ] **Batch Workbench (`EmbryoSplicingBench`):** Bill configuration, disc loading/ejection, `Recipe_BatchApplyBlueprint`.
- [ ] **Embryo Comps & Diagnostics:** `CompEmbryoQuality`, defect generation, `Recipe_BatchScreenEmbryo`, `Recipe_BatchRecycleEmbryo`.
- [ ] **Neural Scanner (`Building_NeuralScanner`):** Colonist neural scanning, `NeuralBlueprintDisk` encoding, fatigue hediff.
- [ ] **Growth Vat Imprinting (`CompGrowthVatImprinter`):** Vat insertion, skill/passion streaming, pawn decanting.
- [ ] **Construct Psychology & Social:** `Gene_ConstructPsychology`, `Trait_ConstructAsset`, romance suppression, death mood suppression.
- [ ] **Persistence & Scribe:** Save/load state corruption, null references upon game load.

---

## 3. Description & Summary

Error Message at the menu screen, game loads without crash, but provides these errors

## 4. Steps to Reproduce

1. Error at launch

---

## 5. Expected Behavior

N/A

---

## 6. Actual Behavior

N/A

---

## 7. Logs & Stack Trace

Paste the relevant exception stack trace from RimWorld's debug console or `Player.log`:
_(Default Linux path: `~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Player.log`)_

```text

Command line arguments: -disable-compute-shaders
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Message (string)
Verse.Root:CheckGlobalInit ()
Verse.Root:Start ()
Verse.Root_Entry:Start ()

RimWorld 1.6.4871 rev600
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Message (string)
RimWorld.VersionControl:LogVersionNumber ()
Verse.Root:CheckGlobalInit ()
Verse.Root:Start ()
Verse.Root_Entry:Start ()

[Eugenics Program] Mod loaded and Harmony patches initialized.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Message (string)
EugenicsProgram.EugenicsProgramMod:.ctor (Verse.ModContentPack)
System.Reflection.RuntimeConstructorInfo:InternalInvoke (object,object[],bool)
System.Reflection.RuntimeConstructorInfo:DoInvoke (object,System.Reflection.BindingFlags,System.Reflection.Binder,object[],System.Globalization.CultureInfo)
System.Reflection.RuntimeConstructorInfo:Invoke (System.Reflection.BindingFlags,System.Reflection.Binder,object[],System.Globalization.CultureInfo)
System.RuntimeType:CreateInstanceImpl (System.Reflection.BindingFlags,System.Reflection.Binder,object[],System.Globalization.CultureInfo,object[],System.Threading.StackCrawlMark&)
System.Activator:CreateInstance (System.Type,System.Reflection.BindingFlags,System.Reflection.Binder,object[],System.Globalization.CultureInfo,object[])
System.Activator:CreateInstance (System.Type,object[])
Verse.LoadedModManager:CreateModClasses ()
Verse.LoadedModManager:LoadAllActiveMods (bool)
Verse.PlayDataLoader:DoPlayLoad ()
Verse.PlayDataLoader:LoadAllPlayData (bool)
Verse.Root/<>c:<Start>b__10_1 ()
Verse.LongEventHandler:RunEventFromAnotherThread (System.Action)
Verse.LongEventHandler/<>c:<UpdateCurrentAsynchronousEvent>b__28_0 ()
System.Threading.ThreadHelper:ThreadStart_Context (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object)
System.Threading.ThreadHelper:ThreadStart ()

XML error: <sterility>true</sterility> doesn't correspond to any field in type GeneDef. Context: <GeneDef><defName>Gene_MandatorySterility</defName><label>mandatory sterility</label><description>Enforces total biological sterility to prevent unauthorized reproduction, gene drift, and protect proprietary colony genetic investments.</description><iconPath>UI/Icons/Genes/Gene_Sterile</iconPath><displayCategory>Eugenics_Construct</displayCategory><displayOrderInCategory>30</displayOrderInCategory><biostatCpx>0</biostatCpx><biostatMet>1</biostatMet><sterility>true</sterility><statFactors><Fertility>0</Fertility></statFactors><customEffectDescriptions><li>Completely sterile. Cannot fertilize or be impregnated.</li><li>Yields +1 Metabolic Efficiency.</li></customEffectDescriptions></GeneDef>
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.XmlToObjectUtils:DoFieldSearch (System.Type,System.Xml.XmlNode,System.Xml.XmlNode)
Verse.DirectXmlToObjectNew:ResolveFieldForNode (System.Type,System.Xml.XmlNode,System.Xml.XmlNode)
(wrapper dynamic-method) Verse.DirectXmlToObjectNew/DummyTypeToHoldDynamicMethods:ParseAndReturnDef_Verse_Def (int,int,System.Xml.XmlNode,System.Type)
Verse.DirectXmlToObjectNew:DefFromNodeNew (System.Xml.XmlNode,Verse.LoadableXmlAsset)
Verse.LoadedModManager:ParseAndProcessXML (System.Xml.XmlDocument,System.Collections.Generic.Dictionary`2<System.Xml.XmlNode, Verse.LoadableXmlAsset>,bool)
Verse.LoadedModManager:LoadAllActiveMods (bool)
Verse.PlayDataLoader:DoPlayLoad ()
Verse.PlayDataLoader:LoadAllPlayData (bool)
Verse.Root/<>c:<Start>b__10_1 ()
Verse.LongEventHandler:RunEventFromAnotherThread (System.Action)
Verse.LongEventHandler/<>c:<UpdateCurrentAsynchronousEvent>b__28_0 ()
System.Threading.ThreadHelper:ThreadStart_Context (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object)
System.Threading.ThreadHelper:ThreadStart ()

XML error: <idlePowerConsumption>50</idlePowerConsumption> doesn't correspond to any field in type CompProperties_Power. Context: <li Class="CompProperties_Power"><compClass>CompPowerTrader</compClass><basePowerConsumption>600</basePowerConsumption><idlePowerConsumption>50</idlePowerConsumption><shortCircuitInRain>true</shortCircuitInRain></li>
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.XmlToObjectUtils:DoFieldSearch (System.Type,System.Xml.XmlNode,System.Xml.XmlNode)
Verse.DirectXmlToObjectNew:ResolveFieldForNode (System.Type,System.Xml.XmlNode,System.Xml.XmlNode)
(wrapper dynamic-method) Verse.DirectXmlToObjectNew/DummyTypeToHoldDynamicMethods:ParseAndAddComplexTypeToList_SharedBody (object,int,System.Xml.XmlNode,System.Type)
(wrapper dynamic-method) Verse.DirectXmlToObjectNew/DummyTypeToHoldDynamicMethods:ParseAndSetListField_Verse_CompProperties (object,System.Reflection.FieldInfo,System.Xml.XmlNode,System.Type)
(wrapper dynamic-method) Verse.DirectXmlToObjectNew/DummyTypeToHoldDynamicMethods:ParseAndReturnDef_Verse_ThingDef (int,int,System.Xml.XmlNode,System.Type)
Verse.DirectXmlToObjectNew:DefFromNodeNew (System.Xml.XmlNode,Verse.LoadableXmlAsset)
Verse.LoadedModManager:ParseAndProcessXML (System.Xml.XmlDocument,System.Collections.Generic.Dictionary`2<System.Xml.XmlNode, Verse.LoadableXmlAsset>,bool)
Verse.LoadedModManager:LoadAllActiveMods (bool)
Verse.PlayDataLoader:DoPlayLoad ()
Verse.PlayDataLoader:LoadAllPlayData (bool)
Verse.Root/<>c:<Start>b__10_1 ()
Verse.LongEventHandler:RunEventFromAnotherThread (System.Action)
Verse.LongEventHandler/<>c:<UpdateCurrentAsynchronousEvent>b__28_0 ()
System.Threading.ThreadHelper:ThreadStart_Context (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object)
System.Threading.ThreadHelper:ThreadStart ()

XML error: <staticSunShadows>true</staticSunShadows> doesn't correspond to any field in type ThingDef. Context: <ThingDef ParentName="BenchBase"><category>Building</category><thingClass>EugenicsProgram.Building_EmbryoSplicingBench</thingClass><soundImpactDefault>BulletImpact_Metal</soundImpactDefault><selectable>true</selectable><drawerType>MapMeshOnly</drawerType><altitudeLayer>Building</altitudeLayer><terrainAffordanceNeeded>Medium</terrainAffordanceNeeded><repairEffect>Repair</repairEffect><leaveResourcesWhenKilled>true</leaveResourcesWhenKilled><filthLeaving>Filth_RubbleBuilding</filthLeaving><statBases><SellPriceFactor>0.70</SellPriceFactor><Mass>20</Mass><MaxHitPoints>350</MaxHitPoints><WorkToBuild>4500</WorkToBuild><Flammability>0.5</Flammability></statBases><canOverlapZones>false</canOverlapZones><minifiedDef>MinifiedThing</minifiedDef><thingCategories><li>BuildingsProduction</li></thingCategories><interactionCellIcon>DiningChair</interactionCellIcon><noRightClickDraftAttack>true</noRightClickDraftAttack><comps><li Class="CompProperties_ReportWorkSpeed"><workSpeedStat>WorkTableWorkSpeedFactor</workSpeedStat></li><li Class="CompProperties_Power"><compClass>CompPowerTrader</compClass><basePowerConsumption>600</basePowerConsumption><idlePowerConsumption>50</idlePowerConsumption><shortCircuitInRain>true</shortCircuitInRain></li><li Class="CompProperties_Flickable" /><li Class="CompProperties_AffectedByFacilities"><linkableFacilities><li>GeneBank</li><li>GeneProcessor</li><li MayRequire="Farxmai2.GeneBanksExapnded">GBE_GeneBankLarge</li><li MayRequire="Farxmai2.GeneBanksExapnded">GBE_GeneBankHuge</li></linkableFacilities></li></comps><placeWorkers><li>PlaceWorker_ReportWorkSpeedPenalties</li></placeWorkers><building><buildingTags><li>Production</li></buildingTags><workTableRoomRole>Workshop</workTableRoomRole></building><defName>EmbryoSplicingBench</defName><label>embryo splicing bench</label><description>A dedicated advanced genetic synthesis workstation designed for batch embryo genetic engineering, prenatal genomic diagnostics, and biomass recycling. Features an integrated master genome blueprint disc drive and facility linking connectors for Gene Banks and Gene Processors.</description><graphicData><texPath>Things/Building/Production/GeneAssembler/GeneAssembler</texPath><graphicClass>Graphic_Multi</graphicClass><drawSize>(3.2, 2.2)</drawSize><shadowData><volume>(2.9, 0.75, 1.8)</volume><offset>(0, 0, 0)</offset></shadowData></graphicData><passability>PassThroughOnly</passability><pathCost>50</pathCost><castEdgeShadows>true</castEdgeShadows><staticSunShadows>true</staticSunShadows><fillPercent>0.5</fillPercent><size>(3, 2)</size><costList><Steel>200</Steel><Plasteel>50</Plasteel><ComponentIndustrial>8</ComponentIndustrial></costList><recipes><li>Eugenics_BatchApplyBlueprint</li><li>Eugenics_BatchScreenEmbryo</li><li>Eugenics_BatchRecycleEmbryo</li></recipes><designationCategory>Biotech</designationCategory><uiOrder>2600</uiOrder><hasInteractionCell>true</hasInteractionCell><interactionCellOffset>(0, 0, -1)</interactionCellOffset><researchPrerequisites><li>Eugenics_ConstructFoundations</li></researchPrerequisites></ThingDef>
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.XmlToObjectUtils:DoFieldSearch (System.Type,System.Xml.XmlNode,System.Xml.XmlNode)
Verse.DirectXmlToObjectNew:ResolveFieldForNode (System.Type,System.Xml.XmlNode,System.Xml.XmlNode)
(wrapper dynamic-method) Verse.DirectXmlToObjectNew/DummyTypeToHoldDynamicMethods:ParseAndReturnDef_Verse_ThingDef (int,int,System.Xml.XmlNode,System.Type)
Verse.DirectXmlToObjectNew:DefFromNodeNew (System.Xml.XmlNode,Verse.LoadableXmlAsset)
Verse.LoadedModManager:ParseAndProcessXML (System.Xml.XmlDocument,System.Collections.Generic.Dictionary`2<System.Xml.XmlNode, Verse.LoadableXmlAsset>,bool)
Verse.LoadedModManager:LoadAllActiveMods (bool)
Verse.PlayDataLoader:DoPlayLoad ()
Verse.PlayDataLoader:LoadAllPlayData (bool)
Verse.Root/<>c:<Start>b__10_1 ()
Verse.LongEventHandler:RunEventFromAnotherThread (System.Action)
Verse.LongEventHandler/<>c:<UpdateCurrentAsynchronousEvent>b__28_0 ()
System.Threading.ThreadHelper:ThreadStart_Context (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object)
System.Threading.ThreadHelper:ThreadStart ()

XML error: <researchPrerequisite>Eugenics_GeneticDiagnostics</researchPrerequisite> doesn't correspond to any field in type WorkGiverDef. Context: <WorkGiverDef><defName>Eugenics_ScreenEmbryo</defName><label>screen embryo genetics</label><giverClass>EugenicsProgram.WorkGiver_ScreenEmbryo</giverClass><workType>Doctor</workType><priorityInType>50</priorityInType><verb>screen</verb><gerund>screening embryos</gerund><requiredCapacities><li>Manipulation</li></requiredCapacities><researchPrerequisite>Eugenics_GeneticDiagnostics</researchPrerequisite></WorkGiverDef>
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.XmlToObjectUtils:DoFieldSearch (System.Type,System.Xml.XmlNode,System.Xml.XmlNode)
Verse.DirectXmlToObjectNew:ResolveFieldForNode (System.Type,System.Xml.XmlNode,System.Xml.XmlNode)
(wrapper dynamic-method) Verse.DirectXmlToObjectNew/DummyTypeToHoldDynamicMethods:ParseAndReturnDef_Verse_Def (int,int,System.Xml.XmlNode,System.Type)
Verse.DirectXmlToObjectNew:DefFromNodeNew (System.Xml.XmlNode,Verse.LoadableXmlAsset)
Verse.LoadedModManager:ParseAndProcessXML (System.Xml.XmlDocument,System.Collections.Generic.Dictionary`2<System.Xml.XmlNode, Verse.LoadableXmlAsset>,bool)
Verse.LoadedModManager:LoadAllActiveMods (bool)
Verse.PlayDataLoader:DoPlayLoad ()
Verse.PlayDataLoader:LoadAllPlayData (bool)
Verse.Root/<>c:<Start>b__10_1 ()
Verse.LongEventHandler:RunEventFromAnotherThread (System.Action)
Verse.LongEventHandler/<>c:<UpdateCurrentAsynchronousEvent>b__28_0 ()
System.Threading.ThreadHelper:ThreadStart_Context (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object)
System.Threading.ThreadHelper:ThreadStart ()

XML error: <researchPrerequisite>Eugenics_GeneticDiagnostics</researchPrerequisite> doesn't correspond to any field in type WorkGiverDef. Context: <WorkGiverDef><defName>Eugenics_RecycleEmbryo</defName><label>liquefy embryo biomass</label><giverClass>EugenicsProgram.WorkGiver_RecycleEmbryo</giverClass><workType>Doctor</workType><priorityInType>40</priorityInType><verb>liquefy</verb><gerund>liquefying embryo biomass</gerund><requiredCapacities><li>Manipulation</li></requiredCapacities><researchPrerequisite>Eugenics_GeneticDiagnostics</researchPrerequisite></WorkGiverDef>
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.XmlToObjectUtils:DoFieldSearch (System.Type,System.Xml.XmlNode,System.Xml.XmlNode)
Verse.DirectXmlToObjectNew:ResolveFieldForNode (System.Type,System.Xml.XmlNode,System.Xml.XmlNode)
(wrapper dynamic-method) Verse.DirectXmlToObjectNew/DummyTypeToHoldDynamicMethods:ParseAndReturnDef_Verse_Def (int,int,System.Xml.XmlNode,System.Type)
Verse.DirectXmlToObjectNew:DefFromNodeNew (System.Xml.XmlNode,Verse.LoadableXmlAsset)
Verse.LoadedModManager:ParseAndProcessXML (System.Xml.XmlDocument,System.Collections.Generic.Dictionary`2<System.Xml.XmlNode, Verse.LoadableXmlAsset>,bool)
Verse.LoadedModManager:LoadAllActiveMods (bool)
Verse.PlayDataLoader:DoPlayLoad ()
Verse.PlayDataLoader:LoadAllPlayData (bool)
Verse.Root/<>c:<Start>b__10_1 ()
Verse.LongEventHandler:RunEventFromAnotherThread (System.Action)
Verse.LongEventHandler/<>c:<UpdateCurrentAsynchronousEvent>b__28_0 ()
System.Threading.ThreadHelper:ThreadStart_Context (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object)
System.Threading.ThreadHelper:ThreadStart ()

Could not resolve cross-reference: No RimWorld.StatDef named CancerRateFactor found to give to RimWorld.StatModifier (null stat)
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.DirectXmlCrossRefLoader/WantedRefForObject:TryResolve (Verse.FailMode)
Verse.DirectXmlCrossRefLoader/<>c__DisplayClass16_0:<ResolveAllWantedCrossReferences>b__0 (Verse.DirectXmlCrossRefLoader/WantedRef)
Verse.GenThreading/<>c__DisplayClass7_1`1<Verse.DirectXmlCrossRefLoader/WantedRef>:<ParallelForEach>b__0 (object)
System.Threading.QueueUserWorkItemCallback:WaitCallback_Context (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.QueueUserWorkItemCallback:System.Threading.IThreadPoolWorkItem.ExecuteWorkItem ()
System.Threading.ThreadPoolWorkQueue:Dispatch ()
System.Threading._ThreadPoolWaitCallback:PerformWaitCallback ()

Could not resolve cross-reference to Verse.ResearchProjectDef named Xenogetics (wanter=prerequisites)
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.DirectXmlCrossRefLoader:TryResolveDef<Verse.ResearchProjectDef> (string,Verse.FailMode,object)
Verse.DirectXmlCrossRefLoader/WantedRefForList`1<Verse.ResearchProjectDef>:TryResolve (Verse.FailMode)
Verse.DirectXmlCrossRefLoader/<>c__DisplayClass16_0:<ResolveAllWantedCrossReferences>b__0 (Verse.DirectXmlCrossRefLoader/WantedRef)
Verse.GenThreading/<>c__DisplayClass7_1`1<Verse.DirectXmlCrossRefLoader/WantedRef>:<ParallelForEach>b__0 (object)
System.Threading.QueueUserWorkItemCallback:WaitCallback_Context (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.QueueUserWorkItemCallback:System.Threading.IThreadPoolWorkItem.ExecuteWorkItem ()
System.Threading.ThreadPoolWorkQueue:Dispatch ()
System.Threading._ThreadPoolWaitCallback:PerformWaitCallback ()

Could not resolve cross-reference: No RimWorld.StatDef named FoodPoisoningChanceFactor found to give to RimWorld.StatModifier (null stat)
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.DirectXmlCrossRefLoader/WantedRefForObject:TryResolve (Verse.FailMode)
Verse.DirectXmlCrossRefLoader/<>c__DisplayClass16_0:<ResolveAllWantedCrossReferences>b__0 (Verse.DirectXmlCrossRefLoader/WantedRef)
Verse.GenThreading/<>c__DisplayClass7_1`1<Verse.DirectXmlCrossRefLoader/WantedRef>:<ParallelForEach>b__0 (object)
System.Threading.QueueUserWorkItemCallback:WaitCallback_Context (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.QueueUserWorkItemCallback:System.Threading.IThreadPoolWorkItem.ExecuteWorkItem ()
System.Threading.ThreadPoolWorkQueue:Dispatch ()
System.Threading._ThreadPoolWaitCallback:PerformWaitCallback ()

Could not resolve cross-reference: No RimWorld.ThoughtDef named AteNutrientPaste found to give to RimWorld.IngestibleProperties RimWorld.IngestibleProperties
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.DirectXmlCrossRefLoader/WantedRefForObject:TryResolve (Verse.FailMode)
Verse.DirectXmlCrossRefLoader/<>c__DisplayClass16_0:<ResolveAllWantedCrossReferences>b__0 (Verse.DirectXmlCrossRefLoader/WantedRef)
Verse.GenThreading/<>c__DisplayClass7_1`1<Verse.DirectXmlCrossRefLoader/WantedRef>:<ParallelForEach>b__0 (object)
System.Threading.QueueUserWorkItemCallback:WaitCallback_Context (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.QueueUserWorkItemCallback:System.Threading.IThreadPoolWorkItem.ExecuteWorkItem ()
System.Threading.ThreadPoolWorkQueue:Dispatch ()
System.Threading._ThreadPoolWaitCallback:PerformWaitCallback ()

Could not resolve cross-reference to Verse.ThingCategoryDef named Genepacks (wanter=thingCategories)
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.DirectXmlCrossRefLoader:TryResolveDef<Verse.ThingCategoryDef> (string,Verse.FailMode,object)
Verse.DirectXmlCrossRefLoader/WantedRefForList`1<Verse.ThingCategoryDef>:TryResolve (Verse.FailMode)
Verse.DirectXmlCrossRefLoader/<>c__DisplayClass16_0:<ResolveAllWantedCrossReferences>b__0 (Verse.DirectXmlCrossRefLoader/WantedRef)
Verse.GenThreading/<>c__DisplayClass7_1`1<Verse.DirectXmlCrossRefLoader/WantedRef>:<ParallelForEach>b__0 (object)
System.Threading.QueueUserWorkItemCallback:WaitCallback_Context (object)
System.Threading.ExecutionContext:RunInternal (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.ExecutionContext:Run (System.Threading.ExecutionContext,System.Threading.ContextCallback,object,bool)
System.Threading.QueueUserWorkItemCallback:System.Threading.IThreadPoolWorkItem.ExecuteWorkItem ()
System.Threading.ThreadPoolWorkQueue:Dispatch ()
System.Threading._ThreadPoolWaitCallback:PerformWaitCallback ()

Mod Test Csharp did not load any content. Following load folders were used:

- /mnt/gaming/SteamLibrary/steamapps/common/RimWorld/Mods/test-csharp
  UnityEngine.StackTraceUtility:ExtractStackTrace ()
  Verse.Log:Error (string)
  Verse.LoadedModManager/<>c:<LoadModContent>b__13_1 ()
  Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
  Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
  Verse.LongEventHandler:LongEventsUpdate (bool&)
  Verse.Root:Update ()
  Verse.Root_Entry:Update ()

Mod Test Mod did not load any content. Following load folders were used:

- /mnt/gaming/SteamLibrary/steamapps/common/RimWorld/Mods/test-mod
  UnityEngine.StackTraceUtility:ExtractStackTrace ()
  Verse.Log:Error (string)
  Verse.LoadedModManager/<>c:<LoadModContent>b__13_1 ()
  Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
  Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
  Verse.LongEventHandler:LongEventsUpdate (bool&)
  Verse.Root:Update ()
  Verse.Root_Entry:Update ()

Failed to find any textures at Things/Building/Production/GeneAssembler/GeneAssembler while constructing Multi(initPath=Things/Building/Production/GeneAssembler/GeneAssembler, color=RGBA(1.000, 1.000, 1.000, 1.000), colorTwo=RGBA(1.000, 1.000, 1.000, 1.000))
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.Graphic_Multi:Init (Verse.GraphicRequest)
Verse.GraphicDatabase:GetInner<Verse.Graphic_Multi> (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (System.Type,string,UnityEngine.Shader,UnityEngine.Vector2,UnityEngine.Color,UnityEngine.Color,Verse.GraphicData,System.Collections.Generic.List`1<Verse.ShaderParameter>,string)
Verse.GraphicData:Init ()
Verse.GraphicData:get_Graphic ()
Verse.ThingDef:<PostLoad>b__398_0 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

Could not load Texture2D at 'Things/Building/Production/SubcoreScanner' for def 'NeuralScanner' in any active mod or in base resources.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.ContentFinder`1<UnityEngine.Texture2D>:Get (string,bool)
Verse.Graphic_Single:Init (Verse.GraphicRequest)
Verse.GraphicDatabase:GetInner<Verse.Graphic_Single> (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (System.Type,string,UnityEngine.Shader,UnityEngine.Vector2,UnityEngine.Color,UnityEngine.Color,Verse.GraphicData,System.Collections.Generic.List`1<Verse.ShaderParameter>,string)
Verse.GraphicData:Init ()
Verse.GraphicData:get_Graphic ()
Verse.ThingDef:<PostLoad>b__398_0 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

MatFrom with null sourceTex.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.MaterialPool:MatFrom (Verse.MaterialRequest)
Verse.Graphic_Single:Init (Verse.GraphicRequest)
Verse.GraphicDatabase:GetInner<Verse.Graphic_Single> (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (System.Type,string,UnityEngine.Shader,UnityEngine.Vector2,UnityEngine.Color,UnityEngine.Color,Verse.GraphicData,System.Collections.Generic.List`1<Verse.ShaderParameter>,string)
Verse.GraphicData:Init ()
Verse.GraphicData:get_Graphic ()
Verse.ThingDef:<PostLoad>b__398_0 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

Could not load Texture2D at 'Things/Item/Meal/NutrientPaste' for def 'GeneticNutrientPaste' in any active mod or in base resources.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.ContentFinder`1<UnityEngine.Texture2D>:Get (string,bool)
Verse.Graphic_Single:Init (Verse.GraphicRequest)
Verse.GraphicDatabase:GetInner<Verse.Graphic_Single> (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (System.Type,string,UnityEngine.Shader,UnityEngine.Vector2,UnityEngine.Color,UnityEngine.Color,Verse.GraphicData,System.Collections.Generic.List`1<Verse.ShaderParameter>,string)
Verse.GraphicData:Init ()
Verse.GraphicData:get_Graphic ()
Verse.ThingDef:<PostLoad>b__398_0 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

MatFrom with null sourceTex.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.MaterialPool:MatFrom (Verse.MaterialRequest)
Verse.Graphic_Single:Init (Verse.GraphicRequest)
Verse.GraphicDatabase:GetInner<Verse.Graphic_Single> (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (System.Type,string,UnityEngine.Shader,UnityEngine.Vector2,UnityEngine.Color,UnityEngine.Color,Verse.GraphicData,System.Collections.Generic.List`1<Verse.ShaderParameter>,string)
Verse.GraphicData:Init ()
Verse.GraphicData:get_Graphic ()
Verse.ThingDef:<PostLoad>b__398_0 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

Could not load Texture2D at 'Things/Item/Special/Genepack' for def 'GenomeBlueprintDisk' in any active mod or in base resources.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.ContentFinder`1<UnityEngine.Texture2D>:Get (string,bool)
Verse.Graphic_Single:Init (Verse.GraphicRequest)
Verse.GraphicDatabase:GetInner<Verse.Graphic_Single> (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (System.Type,string,UnityEngine.Shader,UnityEngine.Vector2,UnityEngine.Color,UnityEngine.Color,Verse.GraphicData,System.Collections.Generic.List`1<Verse.ShaderParameter>,string)
Verse.GraphicData:Init ()
Verse.GraphicData:get_Graphic ()
Verse.ThingDef:<PostLoad>b__398_0 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

MatFrom with null sourceTex.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.MaterialPool:MatFrom (Verse.MaterialRequest)
Verse.Graphic_Single:Init (Verse.GraphicRequest)
Verse.GraphicDatabase:GetInner<Verse.Graphic_Single> (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (System.Type,string,UnityEngine.Shader,UnityEngine.Vector2,UnityEngine.Color,UnityEngine.Color,Verse.GraphicData,System.Collections.Generic.List`1<Verse.ShaderParameter>,string)
Verse.GraphicData:Init ()
Verse.GraphicData:get_Graphic ()
Verse.ThingDef:<PostLoad>b__398_0 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

Could not load Texture2D at 'Things/Item/Special/Genepack' for def 'NeuralBlueprintDisk' in any active mod or in base resources.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.ContentFinder`1<UnityEngine.Texture2D>:Get (string,bool)
Verse.Graphic_Single:Init (Verse.GraphicRequest)
Verse.GraphicDatabase:GetInner<Verse.Graphic_Single> (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (System.Type,string,UnityEngine.Shader,UnityEngine.Vector2,UnityEngine.Color,UnityEngine.Color,Verse.GraphicData,System.Collections.Generic.List`1<Verse.ShaderParameter>,string)
Verse.GraphicData:Init ()
Verse.GraphicData:get_Graphic ()
Verse.ThingDef:<PostLoad>b__398_0 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

MatFrom with null sourceTex.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.MaterialPool:MatFrom (Verse.MaterialRequest)
Verse.Graphic_Single:Init (Verse.GraphicRequest)
Verse.GraphicDatabase:GetInner<Verse.Graphic_Single> (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (System.Type,string,UnityEngine.Shader,UnityEngine.Vector2,UnityEngine.Color,UnityEngine.Color,Verse.GraphicData,System.Collections.Generic.List`1<Verse.ShaderParameter>,string)
Verse.GraphicData:Init ()
Verse.GraphicData:get_Graphic ()
Verse.ThingDef:<PostLoad>b__398_0 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

Failed to find any textures at Things/Building/Production/GeneAssembler/GeneAssembler while constructing Multi(initPath=Things/Building/Production/GeneAssembler/GeneAssembler, color=RGBA(0.824, 0.922, 1.000, 0.600), colorTwo=RGBA(1.000, 1.000, 1.000, 1.000))
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.Graphic_Multi:Init (Verse.GraphicRequest)
Verse.GraphicDatabase:GetInner<Verse.Graphic_Multi> (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (System.Type,string,UnityEngine.Shader,UnityEngine.Vector2,UnityEngine.Color,UnityEngine.Color,Verse.GraphicData,System.Collections.Generic.List`1<Verse.ShaderParameter>,string)
Verse.GraphicData:Init ()
Verse.GraphicData:get_Graphic ()
Verse.ThingDef:<PostLoad>b__398_0 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

Could not load Texture2D at 'Things/Building/Production/SubcoreScanner' for def 'Blueprint_NeuralScanner' in any active mod or in base resources.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.ContentFinder`1<UnityEngine.Texture2D>:Get (string,bool)
Verse.Graphic_Single:Init (Verse.GraphicRequest)
Verse.GraphicDatabase:GetInner<Verse.Graphic_Single> (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (System.Type,string,UnityEngine.Shader,UnityEngine.Vector2,UnityEngine.Color,UnityEngine.Color,Verse.GraphicData,System.Collections.Generic.List`1<Verse.ShaderParameter>,string)
Verse.GraphicData:Init ()
Verse.GraphicData:get_Graphic ()
Verse.ThingDef:<PostLoad>b__398_0 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

MatFrom with null sourceTex.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Error (string)
Verse.MaterialPool:MatFrom (Verse.MaterialRequest)
Verse.Graphic_Single:Init (Verse.GraphicRequest)
Verse.GraphicDatabase:GetInner<Verse.Graphic_Single> (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (Verse.GraphicRequest)
Verse.GraphicDatabase:Get (System.Type,string,UnityEngine.Shader,UnityEngine.Vector2,UnityEngine.Color,UnityEngine.Color,Verse.GraphicData,System.Collections.Generic.List`1<Verse.ShaderParameter>,string)
Verse.GraphicData:Init ()
Verse.GraphicData:get_Graphic ()
Verse.ThingDef:<PostLoad>b__398_0 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

[BiotechCloning] Patching ...
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Message (string)
Dark.Cloning.LoadHarmony:.cctor ()
System.Runtime.CompilerServices.RuntimeHelpers:RunClassConstructor (System.RuntimeTypeHandle)
Verse.StaticConstructorOnStartupUtility:CallAll ()
Verse.PlayDataLoader/<>c:<DoPlayLoad>b__4_4 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

[RecycleThis]: Added 3 things to destroy-benches, 3 things to scrap-benches and 1 things to smelt-benches
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Verse.Log:Message (string)
RecycleThis.RecycleThisUtility:.cctor ()
System.Runtime.CompilerServices.RuntimeHelpers:RunClassConstructor (System.RuntimeTypeHandle)
RecycleThis.RecycleThis:.cctor ()
System.Runtime.CompilerServices.RuntimeHelpers:RunClassConstructor (System.RuntimeTypeHandle)
Verse.StaticConstructorOnStartupUtility:CallAll ()
Verse.PlayDataLoader/<>c:<DoPlayLoad>b__4_4 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
Verse.Root:Update ()
Verse.Root_Entry:Update ()

```

---

## 8. Root Cause Analysis (Dev Notes)

- **Suspected File / Method:**
  - `Defs/GeneDefs/Genes_Construct.xml`: Field `<sterility>` instead of `<sterilize>`.
  - `Defs/GeneDefs/Genes_Optimizer.xml`: Stat name `<CancerRateFactor>` instead of `<CancerRate>`.
  - `Defs/ResearchProjectDefs/ResearchProjects_Eugenics.xml`: Typo `Xenogetics` instead of `Xenogenetics`.
  - `Defs/WorkGiverDefs/WorkGivers_Eugenics.xml`: Unsupported `<researchPrerequisite>` tags on `WorkGiverDef` instances.
  - `Defs/ThingDefs_Buildings/Buildings_EugenicsBench.xml`: Incorrect `texPath` (`Production` instead of `Misc`), invalid `ThingDef` field `<staticSunShadows>`, and `<idlePowerConsumption>` instead of `<idlePowerDraw>`.
  - `Defs/ThingDefs_Buildings/Buildings_Neural.xml`: Incorrect `texPath` `SubcoreScanner` instead of `SubcoreSoftscanner` and `graphicClass` `Graphic_Single` instead of `Graphic_Multi`.
  - `Defs/ThingDefs_Items/Items_Biomass.xml`: `GeneticNutrientPaste` used `Graphic_Single` instead of `Graphic_MealVariants`, non-existent `FoodPoisoningChanceFactor` stat, and `tasteThought` instead of `ateEvent`.
  - `Defs/ThingDefs_Items/Items_Blueprints.xml`: Non-existent `Genepacks` ThingCategoryDef (changed to `ItemsMisc`), and invalid multi-pack `Genepack` texture path with `Graphic_Single` (updated to `Things/Item/Resource/SubcoreRegular` and `Things/Item/Resource/SubcoreHigh`).

- **Proposed Fix:**
  - Corrected XML tags, stat names, research prerequisite names, category references, and texture paths across all affected Def files.
  - Updated `EugenicsProgram.csproj` to resolve workshop `0Harmony.dll` from `/mnt/gaming/SteamLibrary/steamapps/workshop/content/294100/2009463077/Current/Assemblies/0Harmony.dll`.
