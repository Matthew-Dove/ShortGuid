# Short Guid

[PM> NuGet\Install-Package md.shortguid](https://www.nuget.org/packages/md.shortguid/)  

A V4 GUID represented in the form of a short(er), base64-encoded, url-safe, string.  
With the *option* to replace version, and variant data for custom flags.  

![Guid Diagram](assets/guid-version-variant-diagram.png)  

By default .net's `Guid.NewGuid()`, creates a **Version "4"**, and **Variant "DCE 1.1, ISO/IEC 11578:1996"** UUID.  
`ShortGuid` replaces these constant values with custom ones, allowing users to set flags in these locations.  

A standard `Guid` has 36 characters: `2249d2d9-e29a-4c7c-9505-256760d13fad`.  
A `ShortGuid` has 22 characters: `2dJJIprifEyVBSVnYNE_rQ`.  

`Guids`, and `ShortGuids` can be converted to one another without any data loss.  
`ShortGuids` are url safe.  

### Short Guid

```cs
var sg1 = new ShortGuid(); // Creates a new ShortGuid, generating a new Guid. 
var sg2 = new ShortGuid(Guid.NewGuid()); // Creates a ShortGuid, from an existing Guid.
var sg3 = ShortGuid.NewGuid(); // Same as the empty constructor, aligns with Guid's api.
```

### Short Guid With Flags

```cs
// Int32 type flags, in the range of 0 - 63 (inclusive), allows for 64 unique values.
var flags = 63;
var sg1 = new ShortGuid(flags);

// As an alternative, enums can be used with up to 64 unique fields.
public enum Values
{
    A = 0,
    B = 1,
    ...
    Y = 62,
    Z = 63
}
var valueFlags = Values.Z;
var sg2 = new ShortGuid<Values>(valueFlags);

// You could use a flags enum with up to 7 combined values.
[Flags]
public enum Flags
{
    A = 0,
    B = 1,
    C = 2,
    D = 4,
    E = 8,
    F = 16,
    G = 32
}
var combinedFlags = Flags.A | Flags.B | Flags.C | Flags.D | Flags.E | Flags.F | Flags.G;
var sg3 = new ShortGuid<Flags>(combinedFlags);
```

### Converting between Guids and Short Guids

```cs
// This example uses the provided extension methods, but it can be done though the ShortGuid type as well. 

Guid guidIn = Guid.NewGuid(); // e.g. "6a54c832-f9c5-4fc5-bdd2-c546fb513499".
int flagsIn = 42;

// Guid to ShortGuid:
string shortGuid = guidIn.ToShortGuid(flagsIn); // e.g. "MshUasX5xa_90sVG_1E0mQ".

// ShortGuid to Guid:
var (guidOut, flagsOut) = shortGuid.ToGuid();

// ShortGuid contains both the original guid, and flags data; while being 2/3 of the size (when comparing the string formats).
Assert.Equal(guidIn, guidOut);
Assert.Equal(flagsIn, flagsOut);
```

# Credits
* [Icon](https://www.flaticon.com/free-icon/bird_2630452) made by [Vitaly Gorbachev](https://www.flaticon.com/authors/vitaly-gorbachev) from [Flaticon](https://www.flaticon.com/)
* [Guid Diagram Example](https://www.uuidtools.com/decode) from uuidtools.

# Changelog

## 1.0.0

* `ShortGuid` type, with the options of using a plain V4 GUID, or one with custom flags.  
