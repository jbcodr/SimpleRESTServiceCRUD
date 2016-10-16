using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Web;

namespace VocaliRESTService
{
    public class TareaTranscripcion : Tarea
    {
        public Transcripcion Transcripcion { get; private set; }
        public TareaTranscripcion(Transcripcion transcripcion)
        {
            this.Transcripcion = transcripcion;
        }

        protected override void Proceso()
        {
            base.Proceso();
            if (Transcripcion != null && Transcripcion.Estado == EstadoTranscripcion.Pendiente)
            {
                InvoxService invox = new InvoxService();
                InvoxService.InvoxResult invoxResult = invox.ProcesarTranscripcion(Transcripcion.IdTranscripcion, Transcripcion.Login, Transcripcion.Fichero);

                // Comprobamos si se ha solicitado cancelación para abortar el proceso.
                if (CancelacionSolicitada)
                {
                    this.EstadoTarea = EstadoTareaEnum.Cancelada;
                    return;
                }

                Resultado = invoxResult;
            }

            Finalizar();
        }
    }

    public class MidnightProcess
    {
        ITranscripcionRepository repository;
        private object thisLock = new object();

        public MidnightProcess()
        {
            Start();
        }
        public void Start()
        {
            ITranscripcionRepository repository = new TranscripcionRepository();
            List<Transcripcion> listaPendientes = repository.SelectPendientes();
            TareaPool tareas = new TareaPool(3);
            tareas.EstadoChanged += Tareas_EstadoChanged;

            foreach (Transcripcion transcripcion in listaPendientes)
            {
                tareas.AddTarea(new TareaTranscripcion(transcripcion));
            }
        }

        private void Tareas_EstadoChanged(Tarea sender, EventArgs e)
        {
            if (sender.EstadoTarea == Tarea.EstadoTareaEnum.Iniciada)
            {
                lock (thisLock)
                {
                    repository.UpdateCU4ini(((TareaTranscripcion)sender).Transcripcion.IdTranscripcion);
                }
            }
            else if (sender.EstadoTarea == Tarea.EstadoTareaEnum.Finalizada)
            {
                if (sender.Resultado != null)
                {
                    lock (thisLock)
                    {
                        InvoxService.InvoxResult invoxResult = (InvoxService.InvoxResult)sender.Resultado;
                        repository.UpdateCU4fin(invoxResult.IdTranscripcion, invoxResult.Error, invoxResult.FechaTranscripcion, invoxResult.TextoTranscripcion);
                    }
                }
            }
        }
    }

    public class TaskPool
    {
        Queue<Transcripcion> cola;
        int maxThreads;

        public TaskPool(List<Transcripcion> transcripciones, int maxThreads = 4)
        {
            this.maxThreads = maxThreads;

        }

        private void Start()
        {


        }
    }

    public class ProcesoMedianoche
    {
        public static void Iniciar()
        {
            BackgroundWorker worker = new BackgroundWorker();

        }
    }
}