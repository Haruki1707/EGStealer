using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace EGStealer
{
    internal static class Compiler
    {
        readonly internal static string MyTempPath = Path.GetTempPath() + @"ShortCutes\";
        private static CodeCompileUnit unit = new CodeCompileUnit();
        private static StringWriter assemblyInfo = new StringWriter();

        static Compiler()
        {
            AddAttribute(typeof(AssemblyTitleAttribute), "%GAME% EGS game shortcut for STEAM");
            AddAttribute(typeof(AssemblyVersionAttribute), Application.ProductVersion);
            AddAttribute(typeof(AssemblyProductAttribute), "EGS process steal");
            AddAttribute(typeof(AssemblyCompanyAttribute), "Haruki1707");
            AddAttribute(typeof(AssemblyCopyrightAttribute), "Haruki1707");
            new CSharpCodeProvider().GenerateCodeFromCompileUnit(unit, assemblyInfo, new CodeGeneratorOptions());
            assemblyInfo.Close();
        }

        private static void AddAttribute(Type type, object value)
        {
            var attr = new CodeTypeReference(type);
            var decl = new CodeAttributeDeclaration(attr, new CodeAttributeArgument(new CodePrimitiveExpression(value)));
            unit.AssemblyCustomAttributes.Add(decl);
        }

        internal static bool Compile(string code, string Output, string game, string URL, string icon)
        {
            CompilerParameters parameters = new CompilerParameters(
                new[] { "mscorlib.dll", "System.Core.dll", "System.dll", "System.Windows.Forms.dll", "System.Drawing.dll", "System.Runtime.InteropServices.dll", "System.Management.dll", $"{Utils.MyAppData}Microsoft.Win32.TaskScheduler.dll", "System.Xml.dll" }
            )
            {
                CompilerOptions = "-win32icon:" + $"\"{icon}\"" +
                    "\n -target:winexe " +
                    "\n /optimize",
                GenerateExecutable = true,
                OutputAssembly = Output
            };

            CompilerResults results = new CSharpCodeProvider().CompileAssemblyFromSource(parameters,
                new[] { code.Replace("%URL%", URL), assemblyInfo.ToString().Replace("%GAME%", game) });

            if (results.Errors.Count > 0)
            {
                string errors = null;
                foreach (CompilerError CompErr in results.Errors)
                {
                    errors = errors +
                                "Line number " + CompErr.Line +
                                ", Error Number: " + CompErr.ErrorNumber +
                                ", '" + CompErr.ErrorText + ";" +
                                Environment.NewLine;
                }
                MessageBox.Show(errors);
                return false;
            }

            return true;
        }
    }
}
