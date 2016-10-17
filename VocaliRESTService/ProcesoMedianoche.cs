using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Web;

namespace VocaliRESTService
{
    /// <summary>
    /// Objeto que hay que instanciar a las 0:00h para procesar las solicitudes pendientes de transcripción.
    /// </summary>
    public class MidnightProcess
    {
        private ITranscripcionRepository repository;
        private object thisLock = new object();

        public MidnightProcess()
        {
            Log.AppendText("Iniciado MidnightProcess()");
            Start();
        }

        /// <summary>
        /// Inicia una cola de tareas limitada a 3 tareas simultáneas.
        /// Al agregar cada tarea esta es iniciada automáticamente si no está al máximo.
        /// Cuando una tarea es terminada la cola ejecuta automáticamente otra si quedan pendientes,
        /// El evento es capturado aquí para actualizar la bbdd con el resultado.
        /// </summary>
        private void Start()
        {
            this.repository = new TranscripcionRepository();
            List<Transcripcion> listaPendientes = repository.SelectPendientes();
            TareaPool tareas = new TareaPool(3);
            tareas.EstadoChanged += Tareas_EstadoChanged;

            foreach (Transcripcion transcripcion in listaPendientes)
            {
                tareas.AddTarea(new TareaTranscripcion(transcripcion));
            }

            tareas.WaitToFinish();
        }

        private void Tareas_EstadoChanged(Tarea sender, Tarea.EstadoChangedEventArgs e)
        {
            if (e.EstadoNew == Tarea.EstadoTareaEnum.Iniciada)
            {
                lock (thisLock)
                {
                    // Actualizamos el registro en bbdd al estado "En Progreso".
                    repository.UpdateCU4ini(((TareaTranscripcion)sender).Transcripcion.IdTranscripcion);
                }
            }
            else if (e.EstadoNew == Tarea.EstadoTareaEnum.Finalizada)
            {
                if (sender.Resultado != null)
                {
                    lock (thisLock)
                    {
                        // Actualizamos el resultado de la transcripción en bbdd.
                        InvoxService.InvoxResult invoxResult = (InvoxService.InvoxResult)sender.Resultado;
                        repository.UpdateCU4fin(invoxResult.IdTranscripcion, invoxResult.Error, invoxResult.FechaTranscripcion, invoxResult.TextoTranscripcion);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Clase que hereda de Tarea, para realizar un proceso de forma asíncrona.
    /// Sobrescribimos el método Proceso() que se ejecutará de forma asíncrona al llamar a Iniciar().
    /// </summary>
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
}