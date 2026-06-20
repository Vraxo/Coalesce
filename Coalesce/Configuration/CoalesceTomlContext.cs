using Tomlyn.Serialization;

namespace Coalesce.Configuration;

[TomlSerializable(typeof(AppOptions))]
internal partial class CoalesceTomlContext : TomlSerializerContext
{
}
