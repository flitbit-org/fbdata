﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FlitBit.Data.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("FlitBit.Data.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Command definitions cannot be executed directly; to execute, use ImmediateExecuteXXXX methods..
        /// </summary>
        internal static string Chk_CannotExecutCommandDefinition {
            get {
                return ResourceManager.GetString("Chk_CannotExecutCommandDefinition", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parameter not defined..
        /// </summary>
        internal static string Chk_ParameterNotDefined {
            get {
                return ResourceManager.GetString("Chk_ParameterNotDefined", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Either the parameter is already defined or there is a naming conflict..
        /// </summary>
        internal static string Chk_ParameterObstructed {
            get {
                return ResourceManager.GetString("Chk_ParameterObstructed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DbContext disposed out of order. Usually this is due to a DbContext being used without a `using` clause..
        /// </summary>
        internal static string Err_DbContextStackDisposedOutOfOrder {
            get {
                return ResourceManager.GetString("Err_DbContextStackDisposedOutOfOrder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DbCommand produced more results than expected..
        /// </summary>
        internal static string Err_DuplicateDbResult {
            get {
                return ResourceManager.GetString("Err_DuplicateDbResult", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DbCommand produced no results..
        /// </summary>
        internal static string Err_EmptyDbResult {
            get {
                return ResourceManager.GetString("Err_EmptyDbResult", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Overlapping use of an IDbExecutable not allowed..
        /// </summary>
        internal static string Err_OverlappingUseOfExecutable {
            get {
                return ResourceManager.GetString("Err_OverlappingUseOfExecutable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type not supported. Expected : .
        /// </summary>
        internal static string Err_TypeNotSupported {
            get {
                return ResourceManager.GetString("Err_TypeNotSupported", resourceCulture);
            }
        }
    }
}
