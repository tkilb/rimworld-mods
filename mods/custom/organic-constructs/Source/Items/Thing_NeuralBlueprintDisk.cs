using UnityEngine;
using Verse;
using RimWorld;

namespace OrganicConstructs
{
    public class Thing_NeuralBlueprintDisk : ThingWithComps
    {
        private Graphic emptyGraphic;
        private Graphic encodedGraphic;

        public const string EmptyTexPath = "Things/Item/Blueprints/NeuralBlueprintDisk_Empty";
        public const string EncodedTexPath = "Things/Item/Blueprints/NeuralBlueprintDisk_Encoded";

        public override Graphic Graphic
        {
            get
            {
                CompNeuralBlueprint comp = GetComp<CompNeuralBlueprint>();
                if (comp != null && comp.IsEncoded)
                {
                    if (encodedGraphic == null)
                    {
                        Shader shader = def?.graphicData?.Graphic?.Shader ?? ShaderDatabase.Cutout;
                        Vector2 drawSize = def?.graphicData?.drawSize ?? Vector2.one;
                        Color color = def?.graphicData?.color ?? Color.white;
                        Color colorTwo = def?.graphicData?.colorTwo ?? Color.white;
                        encodedGraphic = GraphicDatabase.Get<Graphic_Single>(EncodedTexPath, shader, drawSize, color, colorTwo);
                    }
                    return encodedGraphic ?? base.Graphic;
                }

                if (emptyGraphic == null)
                {
                    Shader shader = def?.graphicData?.Graphic?.Shader ?? ShaderDatabase.Cutout;
                    Vector2 drawSize = def?.graphicData?.drawSize ?? Vector2.one;
                    Color color = def?.graphicData?.color ?? Color.white;
                    Color colorTwo = def?.graphicData?.colorTwo ?? Color.white;
                    emptyGraphic = GraphicDatabase.Get<Graphic_Single>(EmptyTexPath, shader, drawSize, color, colorTwo);
                }
                return emptyGraphic ?? base.Graphic;
            }
        }

        public void Notify_StateChanged()
        {
            if (Spawned && Map != null)
            {
                Map.mapDrawer?.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
            }
        }
    }
}
