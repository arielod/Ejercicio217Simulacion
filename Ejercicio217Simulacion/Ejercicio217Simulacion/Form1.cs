using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Ejercicio217Simulacion
{
    public partial class Form1 : Form
    {
        private Random random = new Random();
        private Dictionary<int, double> horaLlegadaPaquete = new Dictionary<int, double>(); // Guardar hora de llegada

        public Form1()
        {
            InitializeComponent();
            SetupDataGridView();
        }

        private void SetupDataGridView()
        {
            dataGridView1.ColumnCount = 21; // Se incrementa el número de columnas
            dataGridView1.Columns[0].Name = "Reloj";
            dataGridView1.Columns[1].Name = "Evento";
            dataGridView1.Columns[2].Name = "Random Llegada";
            dataGridView1.Columns[3].Name = "Tiempo entre llegadas";
            dataGridView1.Columns[4].Name = "Próxima Llegada";
            dataGridView1.Columns[5].Name = "Random Procesamiento";
            dataGridView1.Columns[6].Name = "Tiempo Procesamiento";
            dataGridView1.Columns[7].Name = "Fin Procesamiento";
            dataGridView1.Columns[8].Name = "Tiempo Vuelta";
            dataGridView1.Columns[9].Name = "Fin Vuelta Paquete";
            dataGridView1.Columns[10].Name = "Estado Router";
            dataGridView1.Columns[11].Name = "Cola";
            dataGridView1.Columns[12].Name = "AC paquetes 12 horas";
            dataGridView1.Columns[13].Name = "AC paquetes no procesados";
            dataGridView1.Columns[14].Name = "AC paquetes sin lugar en router";
            dataGridView1.Columns[15].Name = "Estado paquete";
            dataGridView1.Columns[16].Name = "Hora llegada";
            dataGridView1.Columns[17].Name = "Demora";
            dataGridView1.Columns[18].Name = "Cantidad Paquetes Procesados";
            dataGridView1.Columns[19].Name = "Demora Promedio";
            dataGridView1.Columns[20].Name = "Demora Max";
        }

        private void btnSimular_Click(object sender, EventArgs e)
        {
            Simular();
        }

        private void Simular()
        {
            int numSimulaciones;
            if (!int.TryParse(txtNumSimulaciones.Text, out numSimulaciones) || numSimulaciones < 1 || numSimulaciones > 100000)
            {
                MessageBox.Show("Por favor ingrese un número de simulaciones válido (1-100000).");
                return;
            }

            dataGridView1.Rows.Clear();
            double reloj = 0;
            double randomLlegada = random.NextDouble();
            double tiempoEntreLlegadas = -0.005 * Math.Log(1 - randomLlegada);
            double proximaLlegada = tiempoEntreLlegadas;
            double finProcesamiento = double.MaxValue;
            double finVueltaPaquete = double.MaxValue;
            int cola = 0;
            bool routerOcupado = false;

            int acPaquetes12Horas = 0, acPaquetesNoProcesados = 0, acPaquetesSinLugarRouter = 0;
            int numPaquete = 1;
            Queue<int> colaPaquetes = new Queue<int>();
            int paqueteEnProceso = -1;
            int paqueteTiempoVuelta = -1;
            double demora = 0;
            int cantidadPaquetesProcesados = 0;
            double maxDemora = 0;
            double tiempoVuelta1 = 0;
            double horaLlegadaNum = -1;

            // Tabla inicial
            dataGridView1.Rows.Add(reloj, "Inicialización", randomLlegada, tiempoEntreLlegadas, proximaLlegada, "-", "-", "-", "-", "-", "Libre", cola, acPaquetes12Horas, acPaquetesNoProcesados, acPaquetesSinLugarRouter, "-", "-", "-", "-", "-");

            for (int i = 1; i <= numSimulaciones; i++)
            {
                string evento;
                double randomProc = -1, tiempoProc = -1;
                string estadoRouter = routerOcupado ? "Ocupado" : "Libre";
                string estadoPaquete = "-";
                string horaLlegada = "-";
                string tiempoEntre = "-";
                string tiempoVuelta = "-";

                // Modificar Random Llegada en las primeras 10 filas
                if (i <= 11)
                {
                    randomLlegada = random.NextDouble() * 0.001;
                    tiempoEntreLlegadas = -0.005 * Math.Log(1 - randomLlegada);
                }
                else
                {
                    randomLlegada = random.NextDouble();
                    tiempoEntreLlegadas = -0.005 * Math.Log(1 - randomLlegada);
                }

                if (proximaLlegada <= finProcesamiento && proximaLlegada <= finVueltaPaquete)
                {
                    // Evento: LLEGADA
                    reloj = proximaLlegada;
                    int paqueteActual = numPaquete++;
                    evento = $"Llegada Paquete (paquete {paqueteActual})";
                    horaLlegadaNum = reloj;  // Guardamos la hora de llegada
                    horaLlegadaPaquete[paqueteActual] = reloj; // Guardar la hora de llegada del paquete
                    proximaLlegada = reloj + tiempoEntreLlegadas;
                    tiempoEntre = tiempoEntreLlegadas.ToString();

                    if (!routerOcupado)
                    {
                        routerOcupado = true;
                        randomProc = random.NextDouble();
                        tiempoProc = 0.001 + randomProc * (0.0018 - 0.001);
                        finProcesamiento = reloj + tiempoProc;
                        paqueteEnProceso = paqueteActual;
                        estadoPaquete = $"En procesamiento (paquete {paqueteActual})";
                    }
                    else
                    {
                        if (cola == 10 && paqueteTiempoVuelta == -1)
                        {
                            tiempoVuelta = (0 + 30).ToString();
                            finVueltaPaquete = reloj + 30;
                            paqueteTiempoVuelta = paqueteActual;
                            acPaquetesSinLugarRouter++;

                            estadoPaquete = $"En regreso (paquete {paqueteActual})";
                        }
                        else
                        {
                            cola++;
                            estadoPaquete = $"En espera (paquete {paqueteActual})"; // Mostrar número de paquete en espera
                        }

                        colaPaquetes.Enqueue(paqueteActual);
                        acPaquetesNoProcesados++;
                    }
                }
                else if (finProcesamiento <= proximaLlegada && finProcesamiento <= finVueltaPaquete)
                {
                    // Evento: FIN PROCESAMIENTO
                    reloj = finProcesamiento;
                    evento = $"Fin Procesamiento (paquete {paqueteEnProceso})";

                    if (paqueteEnProceso != -1 && horaLlegadaPaquete.ContainsKey(paqueteEnProceso))
                    {
                        horaLlegadaNum = horaLlegadaPaquete[paqueteEnProceso];
                        demora = reloj - horaLlegadaNum;
                    }
                    else
                    {
                        demora = 0;
                    }

                    cantidadPaquetesProcesados++;

                    if (demora > maxDemora)
                    {
                        maxDemora = demora;
                    }

                    if (reloj < 43200) acPaquetes12Horas++;

                    if (cola > 0)
                    {
                        cola--;
                        paqueteEnProceso = colaPaquetes.Dequeue();
                        randomProc = random.NextDouble();
                        tiempoProc = 0.001 + randomProc * (0.0018 - 0.001);
                        finProcesamiento = reloj + tiempoProc;
                        estadoPaquete = $"En procesamiento (paquete {paqueteEnProceso})";
                    }
                    else
                    {
                        finProcesamiento = double.MaxValue;
                        routerOcupado = false;
                        paqueteEnProceso = -1;
                        estadoPaquete = "-";
                    }
                }
                else
                {
                    // Evento: FIN VUELTA PAQUETE
                    reloj = finVueltaPaquete;
                    evento = $"Fin Vuelta Paquete (paquete {paqueteTiempoVuelta})";

                    finVueltaPaquete = double.MaxValue;

                    // Aquí validamos que la hora de llegada del paquete se mantenga igual
                    if (horaLlegadaPaquete.ContainsKey(paqueteTiempoVuelta))
                    {
                        horaLlegada = horaLlegadaPaquete[paqueteTiempoVuelta].ToString(); // Se mantiene la hora de llegada original
                    }

                    if (!routerOcupado)
                    {
                        routerOcupado = true;
                        paqueteEnProceso = paqueteTiempoVuelta;
                        randomProc = random.NextDouble();
                        tiempoProc = 0.001 + randomProc * (0.0018 - 0.001);
                        finProcesamiento = reloj + tiempoProc;
                        estadoPaquete = $"En procesamiento (paquete {paqueteEnProceso})";
                        paqueteTiempoVuelta = -1;
                    }
                    else
                    {
                        cola++;
                        colaPaquetes.Enqueue(numPaquete++);
                        estadoPaquete = $"En espera (paquete {numPaquete - 1})"; // Mostrar el número de paquete en espera
                        acPaquetesNoProcesados++;
                    }
                }

                // Calcular Demora Promedio
                double demoraPromedio = cantidadPaquetesProcesados > 0 ? demora / cantidadPaquetesProcesados : 0;

                // Asignar tiempo vuelta 1 solo cuando se haya calculado el tiempo de vuelta
                tiempoVuelta1 = tiempoVuelta != "-" ? demora : 0;

                // Agregar fila al DataGridView
                dataGridView1.Rows.Add(
                    reloj,
                    evento,
                    evento.Contains("Llegada Paquete") ? randomLlegada.ToString() : "-",
                    evento.Contains("Llegada Paquete") ? tiempoEntre : "-",
                    proximaLlegada,
                    randomProc != -1 ? randomProc.ToString() : "-",
                    tiempoProc != -1 ? tiempoProc.ToString() : "-",
                    finProcesamiento == double.MaxValue ? "-" : finProcesamiento.ToString(),
                    tiempoVuelta,
                    finVueltaPaquete == double.MaxValue ? "-" : finVueltaPaquete.ToString(),
                    routerOcupado ? "Ocupado" : "Libre",
                    cola,
                    acPaquetes12Horas,
                    acPaquetesNoProcesados,
                    acPaquetesSinLugarRouter,
                    estadoPaquete,
                    horaLlegada != "-" ? horaLlegada : "-",
                    demora.ToString(),
                    cantidadPaquetesProcesados,
                    demoraPromedio,
                    maxDemora
                );
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Lógica que se ejecuta cuando se carga el formulario
            txtNumSimulaciones.Text = "100"; // Puedes establecer un valor predeterminado si lo deseas.
        }

        private void btnSimular_Click_1(object sender, EventArgs e)
        {
            // Llamar al método Simular cuando se haga clic en el botón
            Simular();
        }
    }
}
