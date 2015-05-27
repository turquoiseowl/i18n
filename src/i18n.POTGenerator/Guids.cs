// Guids.cs
// MUST match guids.h
using System;

namespace VSPackage.i18n_POTGenerator
{
    static class GuidList
    {
        public const string guidi18n_POTGeneratorPkgString = "698f006d-7893-4be1-bdf2-a51c47349941";
        public const string guidi18n_POTGeneratorCmdSetString = "9cacca9c-fe1e-4742-be38-d12a72b1dff2";

        public static readonly Guid guidi18n_POTGeneratorCmdSet = new Guid(guidi18n_POTGeneratorCmdSetString);
    };
}