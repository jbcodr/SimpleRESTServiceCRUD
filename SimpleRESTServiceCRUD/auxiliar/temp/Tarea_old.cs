using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Threading = System.Threading;
using System.ComponentModel;
using System.Threading;

namespace JavTools.Tools
{
    /// <summary>
    /// Representa una tarea que se ejecuta de forma asíncrona.
    /// </summary>
    public class Tarea : IDisposable
    {
        #region Clases y Enumeraciones personalizadas

        public enum AccionEnum
        {
            Ninguna,
            Suspender,
            Reiniciar,
            Continuar,
            Cancelar,
        }
        public enum EstadoTareaEnum
        {
            Preparada,
            Iniciada,
            Suspendida,
            Finalizada,
            Cancelada,
        }
        public enum ProgresoTareaEnum
        {
            Porcentaje,
            TiempoRestante,
            Indefinido,
        }

        public class EstadoChangedEventArgs : EventArgs
        {
            public EstadoTareaEnum EstadoOld { get; set; }
            public EstadoTareaEnum EstadoNew { get; set; }
            public EstadoChangedEventArgs(EstadoTareaEnum estadoOld, EstadoTareaEnum estadoNew)
            {
                this.EstadoOld = estadoOld;
                this.EstadoNew = estadoNew;
            }
        }
        public class ProgresoChangedEventArgs : EventArgs
        {
            public decimal PorcCompletado { get; private set; }
            public TimeSpan TiempoRestante { get; private set; }
            public string Texto { get; private set; }
            public ProgresoTareaEnum TipoProgreso { get; private set; }

            public ProgresoChangedEventArgs(ProgresoTareaEnum tipoProgreso, double valor, string texto)
            {
                this.TipoProgreso = tipoProgreso;
                this.Texto = texto;

                switch (tipoProgreso)
                {
                    case ProgresoTareaEnum.Porcentaje:
                        PorcCompletado = Convert.ToDecimal(valor);
                        break;
                    case ProgresoTareaEnum.TiempoRestante:
                        TiempoRestante = TimeSpan.FromSeconds(valor);
                        break;
                }
            }
        }

        #endregion

        #region Eventos y Delegados

        public event EstadoChangedEventHandler EstadoChanged;
        public delegate void EstadoChangedEventHandler(Tarea sender, EstadoChangedEventArgs e);

        public virtual event ProgresoChangedEventHandler ProgresoChanged;
        public delegate void ProgresoChangedEventHandler(Tarea sender, ProgresoChangedEventArgs e);

        #endregion

        #region Propiedades

        private EstadoTareaEnum estadoTarea;
        public EstadoTareaEnum EstadoTarea
        {
            get { return estadoTarea; }
            protected set
            {
                if (estadoTarea != value)
                {
                    EstadoChanged?.Invoke(this, new EstadoChangedEventArgs(estadoTarea, value));
                    estadoTarea = value;
                }
            }
        }

        private string name;
        public virtual string Name
        {
            get { return name; }
            protected set { name = value; }
        }

        private int index;
        public virtual int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
            }
        }

        private object resultado;
        public virtual object Resultado
        {
            get
            {
                return resultado;
            }
            protected set
            {
                resultado = value;
            }
        }

        private decimal porcenjaProgreso;
        public decimal PorcentajeProgreso
        {
            get
            {
                return porcenjaProgreso;
            }
            set
            {
                porcenjaProgreso = value;
                ActualizarProgreso();
            }
        }
        private string textoEtiqueta;
        public string TextoEtiqueta
        {
            get { return textoEtiqueta; }
            set
            {
                textoEtiqueta = value;
                ActualizarProgreso();
            }
        }

        #endregion

        #region Variables privadas

        protected AccionEnum accion;
        protected const int sleep = 1000;
        ProgressBar progressBar;
        Label label;
        ToolStripProgressBar toolStripProgressBar;
        ToolStripLabel toolStripLabel;
        Threading.Thread parentThread;
        Control ctrlAsociado;
        List<Control> ctrlNoTocar;

        #endregion

        #region Constructor y evento Load

        public Tarea()
        {
            progressBar = null;
            label = null;
            toolStripProgressBar = null;
            toolStripLabel = null;
            EstadoTarea = EstadoTareaEnum.Preparada;
            accion = AccionEnum.Ninguna;
            ctrlNoTocar = new List<Control>();
            crono = new Cronometro();
        }
        public Tarea(Component[] cmpnts)
            : this()
        {
            SetControlesInfo(cmpnts);
        }

        #endregion

        #region Métodos púlicos

        public virtual void Iniciar()
        {
            Log.NuevoMensaje("Iniciar()");
            if (estadoTarea == EstadoTareaEnum.Preparada)
            {
                Threading.Thread thread = new System.Threading.Thread(new Threading.ThreadStart(Proceso));
                accion = AccionEnum.Ninguna;
                PorcentajeProgreso = 0;
                EstadoTarea = EstadoTareaEnum.Iniciada;
                parentThread = Threading.Thread.CurrentThread;
                thread.Start();
            }
        }
        public virtual void Suspender()
        {
            Log.NuevoMensaje("Suspender()");
            if (estadoTarea == EstadoTareaEnum.Iniciada)
            {
                accion = AccionEnum.Suspender;
                crono.Pause();
            }
        }
        public virtual void Continuar()
        {
            Log.NuevoMensaje("Continuar()");
            if (estadoTarea == EstadoTareaEnum.Suspendida)
            {
                //if (EstadoChanged != null)
                //{ EstadoChanged(this, new EstadoChangedEventArgs(estadoTarea, EstadoTareaEnum.Iniciada)); }
                accion = AccionEnum.Continuar;
                crono.Start();
            }
        }
        public virtual void Reiniciar()
        {
            Log.NuevoMensaje("Reiniciar()");
            if (estadoTarea == EstadoTareaEnum.Iniciada ||
                estadoTarea == EstadoTareaEnum.Suspendida)
            {
                accion = AccionEnum.Reiniciar;
                crono.ReStart();
            }
            else
            {
                accion = AccionEnum.Ninguna;
                EstadoTarea = EstadoTareaEnum.Preparada;
                Iniciar();
                crono.Start();
            }
        }
        public virtual void Cancelar()
        {
            Log.NuevoMensaje("Cancelar()");
            if (estadoTarea == EstadoTareaEnum.Iniciada ||
                estadoTarea == EstadoTareaEnum.Suspendida)
            {
                accion = AccionEnum.Cancelar;
                crono.Stop();
                //while (estadoTarea == EstadoTareaEnum.Iniciada ||
                //    estadoTarea == EstadoTareaEnum.Suspendida)
                //{
                //    Thread.Sleep(100);
                //}
            }
        }

        /// <summary>
        /// Establece los controles informativos de progreso para esta tarea.
        /// </summary>
        /// <param name="cmpnts"></param>
        public void SetControlesInfo(Component[] cmpnts)
        {
            foreach (Component cmpnt in cmpnts)
            {
                if (cmpnt is ProgressBar) { progressBar = (ProgressBar)cmpnt; }
                else if (cmpnt is ToolStripProgressBar) { toolStripProgressBar = (ToolStripProgressBar)cmpnt; }
                else if (cmpnt is Label) { label = (Label)cmpnt; }
                else if (cmpnt is ToolStripLabel) { toolStripLabel = (ToolStripLabel)cmpnt; }
            }
        }
        /// <summary>
        /// Establece los controles que se activarán / desactivarán según
        /// el estado de la tarea.
        /// </summary>
        /// <param name="ctrl">Control contenedor. Todos los controles dependientes se
        /// activarán / desactivarán, pero no, el mismo control.
        /// </param>
        /// <param name="ctrls">Controles que no se tendrán en cuenta en la activación desactivación.</param>
        public void SetControlesDesactivables(Control ctrl, Control[] ctrls)
        {
            if (ctrl != null) { ctrlAsociado = ctrl; }

            if (ctrls != null) { ctrlNoTocar = new List<Control>(ctrls); }
        }
        /// <summary>
        /// Establece los controles que se activarán / desactivarán según
        /// el estado de la tarea.
        /// </summary>
        /// <param name="ctrl">Control contenedor. Todos los controles dependientes se
        /// activarán / desactivarán, pero no, el mismo control.
        /// </param>
        public void SetControlesDesactivables(Control ctrl)
        {
            SetControlesDesactivables(ctrl, null);
        }

        #endregion

        #region Métodos protegidos

        protected virtual void Finalizar()
        {
            Log.NuevoMensaje("Finalizar()");
            PorcentajeProgreso = 1;
            EstadoTarea = EstadoTareaEnum.Finalizada;
        }

        protected virtual void Proceso()
        {
            Log.NuevoMensaje("Proceso()");
            //Finalizar();
        }

        protected virtual void ProcesarAcciones()
        {
            if (!parentThread.IsAlive) { accion = AccionEnum.Cancelar; }
            if (accion == AccionEnum.Suspender)
            {
                EstadoTarea = EstadoTareaEnum.Suspendida;
                while (accion == AccionEnum.Suspender)
                {
                    Threading.Thread.Sleep(sleep);
                }
            }
            switch (accion)
            {
                case AccionEnum.Continuar:
                    accion = AccionEnum.Ninguna;
                    EstadoTarea = EstadoTareaEnum.Iniciada;
                    break;
                case AccionEnum.Cancelar:
                case AccionEnum.Reiniciar:
                    EstadoTarea = EstadoTareaEnum.Cancelada;
                    break;
            }
        }

        #endregion

        #region Métodos privados

        delegate void ActualizarProgresoCallBack();
        private void ActualizarProgreso()
        {
            if (progressBar == null && label == null && toolStripProgressBar == null && toolStripLabel == null) { return; }
            Control control = null;
            if (progressBar != null && progressBar.InvokeRequired) { control = progressBar; }
            else if (label != null && label.InvokeRequired) { control = label; }
            else if (toolStripProgressBar != null && toolStripProgressBar.Owner != null && toolStripProgressBar.Owner.InvokeRequired) { control = toolStripProgressBar.Owner; }
            else if (toolStripLabel != null && toolStripLabel.Owner != null && toolStripLabel.Owner.InvokeRequired) { control = toolStripLabel.Owner; }

            if (control != null)
            {
                control.BeginInvoke(new ActualizarProgresoCallBack(ActualizarProgreso));
                return;
            }

            try
            {
                if (progressBar != null)
                { progressBar.Value = (int)(progressBar.Minimum + porcenjaProgreso * (progressBar.Maximum - progressBar.Minimum)); }
                if (toolStripProgressBar != null)
                { toolStripProgressBar.Value = (int)(toolStripProgressBar.Minimum + porcenjaProgreso * (toolStripProgressBar.Maximum - toolStripProgressBar.Minimum)); }
                if (label != null)
                { label.Text = estadoTarea.ToString()[0] + ": " + porcenjaProgreso.ToString("0.0000%"); }
                if (toolStripLabel != null)
                { toolStripLabel.Text = estadoTarea.ToString()[0] + ": " + porcenjaProgreso.ToString("0.0000%"); }
            }
            catch
            {
                //Cancelar();
            }
        }

        delegate void CambiaControles_CallBack(Control control, bool activar);
        private void CambiaControles(Control control, bool activar)
        {
            if (control == null) { return; }
            if (control.InvokeRequired)
            {
                control.BeginInvoke(new CambiaControles_CallBack(CambiaControles), new object[] { control, activar });
                return;
            }

            if (ctrlNoTocar.Contains(control)) { return; }
            if (control.HasChildren)
            {
                foreach (Control ctrl in control.Controls)
                {
                    CambiaControles(ctrl, activar);
                }
            }
            else
            {
                control.Enabled = activar;
            }
        }

        #endregion

        #region Miembros de IComponent

        public event EventHandler Disposed;

        public System.ComponentModel.ISite Site
        {
            get
            { return null; }
            set
            { ;}
        }

        #endregion

        #region Miembros de IDisposable

        public void Dispose()
        {
            if (estadoTarea == EstadoTareaEnum.Iniciada ||
                estadoTarea == EstadoTareaEnum.Suspendida)
            {
                Cancelar();
            }
            if (Disposed != null) { Dispose(); }
        }

        ~Tarea()
        {
            Dispose();
        }

        #endregion
    }

    public class TareaPool
    {
        ManualResetEvent resetEvent = new ManualResetEvent(false);
        Queue<Tarea> colaTareas = null;
        private object thisLock = new Object();
        private readonly int limite = 1;
        private int count = 0;

        public TareaPool()
        {
            colaTareas = new Queue<Tarea>();
        }

        public void AddTarea(Tarea tarea)
        {
            lock (thisLock)
            {
                tarea.EstadoChanged += new Tarea.EstadoChangedEventHandler(tarea_EstadoChanged);
                if (count < limite)
                {
                    count++;
                    tarea.Iniciar();
                }
                else
                { colaTareas.Enqueue(tarea); }
            }
        }

        private void tarea_EstadoChanged(Tarea sender, Tarea.EstadoChangedEventArgs e)
        {
            if (e.EstadoNew == Tarea.EstadoTareaEnum.Finalizada || e.EstadoNew == Tarea.EstadoTareaEnum.Cancelada)
            {
                lock (thisLock)
                {
                    count--;

                    if (colaTareas.Count > 0)
                    {
                        count++;
                        Tarea tarea = colaTareas.Dequeue();
                        tarea.Iniciar();
                    }

                    if (count == 0)
                    {
                        new Thread(delegate(){
                            resetEvent.Set(); }).Start();

                        //resetEvent.Set(); 
                        //lock (syncRoot)
                        //{
                        //    Monitor.Pulse(syncRoot);
                        //}
                        //waitHandle.Set();
                    }
                }
            }
        }

        /// <summary>
        /// Espera a que finalicen o se cancelen las tareas.
        /// </summary>
        public void WaitToFinish()
        {
            //lock (thisLock)
            //{
                if (count > 0) { resetEvent.WaitOne(); }
                //lock (syncRoot)
                //{
                //    if (count > 0)
                //    {
                //        Monitor.Wait(syncRoot);
                //    }
                //}
                //if (count > 0) { waitHandle.WaitOne(); }
            //}
        }
    }
}
