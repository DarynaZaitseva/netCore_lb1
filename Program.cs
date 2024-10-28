using System.Diagnostics;
using System.Drawing;
using System.Threading;

public class SquareMatrix
{
    private double[,] matrix;
    public SquareMatrix() { this.matrix = new double[3, 3];}
    public SquareMatrix(double[,] m) {
        if(m.GetLength(0) == m.GetLength(1))// Кількість рядків == Кількість стовпців
        { this.matrix = m; }
        else { this.matrix = new double[3, 3]; }
    }
    public SquareMatrix(int size, double minValue = -10.0, double maxValue = 10.0)
    {
        this.matrix = new double[size, size];
        FillWithRandomValues(minValue, maxValue);
    }
    private void FillWithRandomValues(double minValue, double maxValue)
    {
        Random random = new Random();
        for (int i = 0; i < this.matrix.GetLength(0); i++)
        {
            for (int j = 0; j < this.matrix.GetLength(1); j++)
            {
                this.matrix[i, j] = random.NextDouble() * (maxValue - minValue) + minValue;
            }
        }
    }
    public void PrintMatrix()
    {
        Console.WriteLine("Matrix:");
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                Console.Write($"{matrix[i, j]:F2}\t");
            }
            Console.WriteLine();
        }
    }
    public SquareMatrix GetMinor(int row, int col)
    {
        int size = this.matrix.GetLength(0);
        double[,] minor = new double[size - 1, size - 1];

        int mRow = 0, mCol;

        for (int i = 0; i < size; i++)
        {
            if (i == row) { continue; }
            else { mCol = 0; }
            for (int j = 0; j < size; j++)
            {
                if (j == col) { continue; }
                else
                {
                    minor[mRow, mCol] = this.matrix[i, j];
                    mCol++;
                }
            }
            mRow++;
        }

        return new SquareMatrix(minor);
    }
    public double GetDet_SingleThread()
    {
        if(this.matrix.GetLength(0) == 2)
        {
            return this.matrix[0,0]* this.matrix[1,1] - this.matrix[0,1]* this.matrix[1,0];
        }
        else
        {
            double det = 0;
            for(int j = 0; j < this.matrix.GetLength(0); j++)
            {
                SquareMatrix minor = this.GetMinor(0,j); 
                det += Math.Pow(-1, j) * this.matrix[0, j] * minor.GetDet_SingleThread();                
            }
            return det;
        }
    }
    public double GetDet_MultiThread(int num_of_threads)
    {
        double det = 0;
        int size = this.matrix.GetLength(0);
        double[] partial_results = new double[num_of_threads]; //для збереження часткових результатів
        Thread[] threads = new Thread[num_of_threads]; //масив потоків
        object lockObject = new object(); // об'єкт для блокування

        void CalculateDet(int threadIndex) //функція, яку буде використовувати кожен потік
        {

            double part_result = 0;
            
            for (int j = threadIndex; j < size; j += num_of_threads)
            {
                SquareMatrix minor = this.GetMinor(0, j);
                part_result += Math.Pow(-1, j) * this.matrix[0, j] * minor.GetDet_SingleThread();

            }
            partial_results[threadIndex] = part_result;
        }
        
        for (int i = 0; i < num_of_threads; i++) //створюю і запускаю потоки
        {
            int thread_index = i;
            threads[i] = new Thread(() => CalculateDet(thread_index));
            threads[i].Start();
        }
        
        for (int i = 0; i < num_of_threads; i++) { threads[i].Join(); } //чекаю, поки всі потоки порахують

        for (int i = 0; i < num_of_threads; i++) { det += partial_results[i]; } //обчислюю загальний визначник

        return det;
    }
    public void Test(int max_num_of_threads)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        stopwatch.Start();
        double single_thread_det = this.GetDet_SingleThread();
        stopwatch.Stop();
        double single_thread_time = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"Single thread time: {single_thread_time} мс, Determinant: {single_thread_det}");
        for (int i = 2; i <= max_num_of_threads; i++)
        {
            stopwatch.Restart();
            double multi_thread_det = this.GetDet_MultiThread(i);
            stopwatch.Stop();
            double multi_thread_time = stopwatch.ElapsedMilliseconds;
            
            Console.WriteLine($"Multi-threaded time: {multi_thread_time} мс, Determinant: {multi_thread_det}, Number of threads: {i}");
            Console.WriteLine($"Acceleration: {single_thread_time / multi_thread_time}");
        }

    }
}


public class Program
{
    public static void Main()
    {
        //double[,] m =
        //{
        //    { 4, 3, 2 },
        //    { 3, 1, -1 },
        //    { 2, -1, 1 }
        //};

        SquareMatrix matrix = new SquareMatrix(10);
        matrix.PrintMatrix();
        Console.WriteLine();
        matrix.Test(6);

    }
}


