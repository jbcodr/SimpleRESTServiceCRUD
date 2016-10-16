using System;
using System.Collections.Generic;
using System.Threading;

namespace VocaliRESTService
{
    /// <summary>
    /// Representa una tarea que se ejecuta de forma asíncrona.
    /// </summary>
    public abstract class Tarea
    {
        #region Clases y Enumeraciones personalizadas

        public enum EstadoTareaEnum
        {
            Preparada,
            Iniciada,
            Finalizada,
            Cancelada,
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

        #endregion

        #region Eventos y Delegados

        public event EstadoChangedEventHandler EstadoChanged;
        public delegate void EstadoChangedEventHandler(Tarea sender, EstadoChangedEventArgs e);

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

        public virtual object Resultado { get; protected set; }

        protected bool CancelacionSolicitada { get; set; }

        #endregion

        #region Variables

        protected const int sleep = 1000;

        #endregion

        #region Constructor y evento Load

        public Tarea()
        {
            EstadoTarea = EstadoTareaEnum.Preparada;
        }

        #endregion

        #region Métodos púlicos

        public virtual void Iniciar()
        {
            if (estadoTarea == EstadoTareaEnum.Preparada)
            {
                Thread thread = new Thread(new ThreadStart(Proceso));
                EstadoTarea = EstadoTareaEnum.Iniciada;
                thread.Start();
            }
        }
        public void Cancelar()
        {
            if (estadoTarea == EstadoTareaEnum.Iniciada)
            {
                CancelacionSolicitada = true;
            }
        }

        #endregion

        #region Métodos virtuales

        protected virtual void Finalizar()
        {
            EstadoTarea = EstadoTareaEnum.Finalizada;
        }

        protected virtual void Proceso()
        {

        }

        #endregion
    }
    /// <summary>
    /// Representa una cola de tareas que se ejecutarán simultáneamente.
    /// </summary>
    public class TareaPool
    {
        #region Eventos y Delegados

        public event EstadoChangedEventHandler EstadoChanged;
        public delegate void EstadoChangedEventHandler(Tarea sender, EventArgs e);

        #endregion

        ManualResetEvent resetEvent = new ManualResetEvent(false);
        Queue<Tarea> colaTareas;
        private object thisLock = new object();
        private int maxThreads;
        private int numThreads;

        public TareaPool(int maxThreads = 4)
        {
            this.maxThreads = maxThreads;
            colaTareas = new Queue<Tarea>();
        }

        public void AddTarea(Tarea tarea)
        {
            lock (thisLock)
            {
                tarea.EstadoChanged += new Tarea.EstadoChangedEventHandler(tarea_EstadoChanged);
                if (numThreads < maxThreads)
                {
                    numThreads++;
                    tarea.Iniciar();
                }
                else
                { colaTareas.Enqueue(tarea); }
            }
        }

        private void tarea_EstadoChanged(Tarea sender, Tarea.EstadoChangedEventArgs e)
        {
            EstadoChanged?.Invoke(sender, null);
            if (e.EstadoNew == Tarea.EstadoTareaEnum.Finalizada || e.EstadoNew == Tarea.EstadoTareaEnum.Cancelada)
            {
                lock (thisLock)
                {
                    numThreads--;

                    if (colaTareas.Count > 0)
                    {
                        numThreads++;
                        Tarea tarea = colaTareas.Dequeue();
                        tarea.Iniciar();
                    }

                    if (numThreads == 0)
                    {
                        new Thread(delegate () {
                            resetEvent.Set();
                        }).Start();
                    }
                }
            }
        }

        /// <summary>
        /// Espera a que finalicen o se cancelen las tareas.
        /// </summary>
        public void WaitToFinish()
        {
            if (numThreads > 0) { resetEvent.WaitOne(); }
        }
    }
}
