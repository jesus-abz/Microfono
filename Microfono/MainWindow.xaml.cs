using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NAudio;
using NAudio.Wave;
using NAudio.Dsp;

namespace Microfono
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WaveIn waveIn; //conexion con microfono
        WaveFormat formato; //formato de audio

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnIniciar_Click(object sender, RoutedEventArgs e)
        {
            //inicializar conexion
            waveIn = new WaveIn();

            //establecer formato
            waveIn.WaveFormat = new WaveFormat(44100, 16, 1);
            formato = waveIn.WaveFormat;

            //duracion del buffer
            waveIn.BufferMilliseconds = 500;

            //con que funcion respondemos cuando se llena el buffer
            waveIn.DataAvailable += WaveIn_DataAvailable;

            waveIn.StartRecording();
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytesGrabados = e.BytesRecorded;

            int numMuestras = bytesGrabados / 2;

            int exponente = 1;
            int numeroBits = 0;

            do
            {
                exponente++;
                numeroBits = (int)Math.Pow(2, exponente);
            } while (numeroBits < numMuestras);
            exponente -= 1;

            Complex[] muestrasComplejas = new Complex[numeroBits];

            for (int i = 0; i < bytesGrabados; i += 2)
            {
                short muestra = (short)(buffer[i + 1] << 8 | buffer[i]);
                float muestra32Bits = (float)muestra / 32768.0f;
                //muestrasComplejas[i].X = muestra32Bits;
                if (i/2 < numeroBits)
                {
                    muestrasComplejas[i / 2].X = muestra32Bits;
                }
            }

            FastFourierTransform.FFT(true, exponente, muestrasComplejas);

            float[] valoresAbsolutos = new float[muestrasComplejas.Length];

            for(int i=0; i < muestrasComplejas.Length; i++)
            {
                valoresAbsolutos[i] = (float) Math.Sqrt((muestrasComplejas[i].X * muestrasComplejas[i].X) +
                                                        (muestrasComplejas[1].Y * muestrasComplejas[i].Y));
            }

            int indiceValorMaximo = valoresAbsolutos.ToList().IndexOf(valoresAbsolutos.Max());

            float frecuenciaFundamental = (float)(indiceValorMaximo * formato.SampleRate) / (float)valoresAbsolutos.Length;

            lblHertz.Text = frecuenciaFundamental.ToString("n") + " Hz";

        }

        private void BtnDetener_Click(object sender, RoutedEventArgs e)
        {
            waveIn.StopRecording();
        }
    }
}
