using Ninject;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StinterLogger
{
    public interface ITest
    {
        string OutputString();
    }

    public class Test : ITest
    {
        public string OutputString()
        {
            return "asdasdasd";
        }
    }

    public class Mobule : Ninject.Modules.NinjectModule
    {
        public override void Load()
        {
            Bind<ITest>().To<Test>().InSingletonScope();
            Bind<Window>().To<MainWindow>().InTransientScope();
        }
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IKernel _kernel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _kernel = new StandardKernel();
            _kernel.Load(new Mobule());

            var test = new Ninject.Parameters.ConstructorArgument("test", new Test());
            var mw = _kernel.Get<MainWindow>(test);
            mw.Show();
            //Current.MainWindow.Show();
        }
    }
}
