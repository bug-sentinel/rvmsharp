namespace CadRevealComposer.Primitives
{
    using Newtonsoft.Json;
    using System.Numerics;

    public record Ring(
        CommonPrimitiveProperties CommonPrimitiveProperties,
        [property: I3df(I3dfAttribute.AttributeType.Normal)]
        [property: JsonProperty("normal")] Vector3 Normal,
        [property: I3df(I3dfAttribute.AttributeType.Radius)]
        [property: JsonProperty("inner_radius")]
        float InnerRadius,
        [property: I3df(I3dfAttribute.AttributeType.Radius)]
        [property: JsonProperty("outer_radius")]
        float OuterRadius
    ) : APrimitive(CommonPrimitiveProperties);
}