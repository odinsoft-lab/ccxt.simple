# TASK.md - Pending Tasks and Work in Progress

## System.Text.Json Migration (Status: In Progress - Postponed)

### Overview
Migration from Newtonsoft.Json to System.Text.Json for the entire CCXT.Simple project.

### Current Status
- **Date Started**: 2024-08-07
- **Status**: Partially completed, postponed for later continuation
- **Progress**: ~50% - Basic migration script created and executed

### Completed Work
1. ✅ Created migration PowerShell script (`migrate-to-system-text-json.ps1`)
2. ✅ Updated project file to include System.Text.Json package reference
3. ✅ Updated GlobalUsings.cs with System.Text.Json namespaces
4. ✅ Created JsonExtensions helper class in `src/Extensions/JsonExtensions.cs`
5. ✅ Modified 124 out of 247 source files with basic replacements:
   - Replaced `using Newtonsoft.Json` → `using System.Text.Json`
   - Replaced `using Newtonsoft.Json.Linq` → `using System.Text.Json.Nodes`
   - Replaced `JObject` → `JsonObject`
   - Replaced `JArray` → `JsonArray`
   - Replaced `JToken` → `JsonNode`
   - Replaced `JsonConvert.DeserializeObject` → `JsonSerializer.Deserialize`
   - Replaced `JObject.Parse()` → `JsonNode.Parse()?.AsObject()`
   - Replaced `JArray.Parse()` → `JsonNode.Parse()?.AsArray()`

### Remaining Work
1. ⏳ Fix compilation errors from the migration
2. ⏳ Handle special cases that need manual review:
   - Files with `JsonSerializerSettings` references
   - Files with custom `JsonConverter` implementations
   - Files with `JsonConstructor` attributes
   - Complex LINQ operations on JsonArray/JsonObject

### Files Requiring Manual Review
The following files contain patterns that need manual attention:
- `src/Data/DecimalConverter.cs` - Custom JsonConverter implementation
- `src/Exchanges/Exchange.cs` - JsonSerializerSettings usage
- Multiple exchange implementations with complex JSON parsing logic

### Known Issues to Address
1. **JsonArray LINQ Operations**: 
   - `JsonArray.Take()` doesn't exist - need to use `.Take()` as LINQ extension
   - `SingleOrDefault()` with predicates needs adjustment for JsonNode syntax

2. **Value Access Patterns**:
   - `jobject.Value<T>("property")` → `jobject.GetValue<T>("property")`
   - `jarray[index]["property"].ToString()` → `jarray[index]?["property"]?.GetValue<string>()`

3. **Properties Access**:
   - `JObject.Properties()` → `JsonObject.AsEnumerable()`

4. **Type Checking**:
   - Need to handle nullable JsonNode returns from Parse operations

### How to Resume
1. Run `dotnet build` to see current compilation errors
2. Fix remaining compilation errors using the patterns documented above
3. Update custom converters in `src/Data/DecimalConverter.cs` to use System.Text.Json
4. Test all exchange implementations thoroughly
5. Update unit tests if any are affected

### Migration Script Location
The PowerShell migration script is saved at: `D:\github.com\lisa3907\ccxt.simple\migrate-to-system-text-json.ps1`

### Notes
- The migration was postponed after initial automated conversion
- GraphQL.Client.Serializer.SystemTextJson is already in use, which is compatible
- Need to ensure all JSON serialization options are properly configured in JsonExtensions class
- Consider performance implications and test thoroughly before final deployment

---

## Other Pending Tasks

### Folder Reorganization (Status: Completed)
- ✅ Moved `DecimalConverter.cs` and `SideTypeConverter.cs` from Converters to Data folder
- ✅ Moved `Extensions.cs` from Services to Extensions folder
- ✅ Updated all namespace references (121 files)
- ✅ Deleted empty Converters folder

---

*Last Updated: 2024-08-07*