namespace SubSonic.Core.VisualStudio.Attributes
{
    using Microsoft.VisualStudio.Shell;
    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ProvideAssemblyObjectAttribute : RegistrationAttribute
    {
        private readonly Type _objectType;
        private RegistrationMethod _registrationMethod;

        public ProvideAssemblyObjectAttribute(Type objectType)
            : base()
        {
            _objectType = objectType ?? throw new ArgumentNullException("objectType");
        }

        public Type ObjectType
        {
            get
            {
                return _objectType;
            }
        }

        public RegistrationMethod RegistrationMethod
        {
            get
            {
                return _registrationMethod;
            }

            set
            {
                _registrationMethod = value;
            }
        }

        private string ClsidRegKey
        {
            get
            {
                return string.Format(@"CLSID\{0}", ObjectType.GUID.ToString("B"));
            }
        }

        private string CodeBase
        {
            get
            {
                return string.Format(@"$PackageFolder$\{0}.dll", ObjectType.Assembly.GetName().Name);
            }
        }

        public override void Register(RegistrationContext context)
        {
            using (Key key = context.CreateKey(ClsidRegKey))
            {
                key.SetValue(string.Empty, ObjectType.FullName);
                key.SetValue("InprocServer32", context.InprocServerPath);
                key.SetValue("Class", ObjectType.FullName);
                if (context.RegistrationMethod != RegistrationMethod.Default)
                    _registrationMethod = context.RegistrationMethod;

                switch (RegistrationMethod)
                {
                    case Microsoft.VisualStudio.Shell.RegistrationMethod.Default:
                    case Microsoft.VisualStudio.Shell.RegistrationMethod.Assembly:
                        key.SetValue("Assembly", ObjectType.Assembly.FullName);
                        break;

                    case Microsoft.VisualStudio.Shell.RegistrationMethod.CodeBase:
                        key.SetValue("CodeBase", CodeBase);
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                key.SetValue("ThreadingModel", "Both");
            }
        }

        public override void Unregister(RegistrationContext context)
        {
            context.RemoveKey(ClsidRegKey);
        }
    }
}
