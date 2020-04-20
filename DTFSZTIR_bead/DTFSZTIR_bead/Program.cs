using System;
using System.Collections.Generic;
using System.Linq;

//Feladat, ahol a < a szummát jelöli
//F, Calm | perm, si,j,m, Trk,l | [Cmax, Tmax,<Ti,<Ui]

namespace DTFSZTIR_bead
{
    class Program
    {
        public struct Job
        {
            public int id;
            public int[] procT;
            public int[] startT;
            public int[] endT;
            public int dueD;
        }

        public struct TimeWindow
        {
            public int ST;
            public int ET;
        }
        public struct Resourse
        {
            public int id;
            public int[] transT; //anyagmozg
            public int[,] setT;//átállás
            public int intervalN;
            public TimeWindow[] interval;
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!We love unicorns!");
            double[] f = new double[4]; //célfgvek
            int[] w = new int[4];//célfgvek prioritása

            int numberJob; //munkak szama
            int numberRes; //erőf száma
            Job[] job; // munkak
            Resourse[] res; //erőforrások
            int[] s; //kozos utemterv
            int return_value;
            int c; //intervallum index
            int cut_mode; //operációk TW-ra illesztése: 0 nem megszakítható, 1 megszakítható

            Console.WriteLine("Feladatunk: F, Calm | perm, si,j,m, Trk,l | [Cmax, Tmax,∑Ti,∑Ui]");
            Console.WriteLine("Adja meg a munkak szamat!");
            numberJob = int.Parse(Console.ReadLine());

            Console.WriteLine("Adja meg az eroforrasok szamat!");
            numberRes = int.Parse(Console.ReadLine());

            Console.WriteLine("Célfuggvenyek prioritasa:");
            for (int i = 0; i < w.Length; i++)
            {
                Console.WriteLine(i + 1 + ". prioritas:");
                w[i] = int.Parse(Console.ReadLine());
            }

            Console.WriteLine("Az operációk megszakíthatók? (1-igen, 0-nem)");
            cut_mode = int.Parse(Console.ReadLine());

            res = new Resourse[numberRes];
            Random rand = new Random();
            for (int i = 0; i < numberRes; i++)
            {
                res[i].id = i;
                res[i].transT = new int[numberRes];
                for (int j = 0; j < numberRes; j++)
                {
                    res[i].transT[j] = rand.Next(10, 20);
                }

                res[i].setT = new int[numberJob, numberJob];
                for (int k = 0; k < numberJob; k++)
                {
                    for (int l = 0; l < numberJob; l++)
                    {
                        if (k == l)
                        {
                            res[i].setT[k, l] = 0;
                        }
                        else
                        {
                            res[i].setT[k, l] = rand.Next(10, 100);
                        }

                    }
                }

                //intervallumok
                res[i].intervalN = rand.Next(2, 10);
                res[i].interval = new TimeWindow[res[i].intervalN];
                for (int m = 0; m < res[i].intervalN; m++)
                {
                    if (m==0)
                    {
                        res[i].interval[m].ST = rand.Next(10, 30);
                    }
                    else
                    {
                        res[i].interval[m].ST = rand.Next(res[i].interval[m - 1].ET, res[i].interval[m - 1].ET+30); 
                    }
                    res[i].interval[m].ET = rand.Next(res[i].interval[m].ST, res[i].interval[m].ST+100);
                }
            }

            s = new int[numberJob];
            job = new Job[numberJob];
            for (int i = 0; i < numberJob; i++)
            {
                job[i].id = i;
                job[i].procT = new int[numberRes]; //gépenkénti muv idő
                for (int j = 0; j < numberRes; j++)
                {
                    job[i].procT[j] = rand.Next(1, 100);
                }
                job[i].startT = new int[numberRes];
                job[i].endT = new int[numberRes];

                //határidő
                job[i].dueD = rand.Next(100, 5000);
                s[i] = numberJob - i - 1;
            }

            //ad-hoc sorrend
            Console.WriteLine("\nAd-hoc sorrend:");
            Simulation(job, numberJob, res, numberRes, s, 0, cut_mode);
            Console.WriteLine("Cmax értéke:{0}", job[s[numberJob - 1]].endT[numberRes - 1]);

            //Johnson_alg - F2
            Console.WriteLine("\nJohnson-algoritmus:");
            Johnson_alg(job,numberJob,0,s);
            Simulation(job, numberJob, res, numberRes, s, 0, cut_mode);
            Console.WriteLine("Cmax értéke:{0}", job[s[numberJob - 1]].endT[numberRes - 1]);

            //Kiterjesztett Johnson-alg - F3
            Console.WriteLine("\nKiterjesztett Johnson-algoritmus:");
            Johnson3_alg(job, numberJob, 0, s);
            Simulation(job, numberJob, res, numberRes, s, 0, cut_mode);
            Console.WriteLine("Cmax értéke:{0}", job[s[numberJob - 1]].endT[numberRes - 1]);

            //Palmer - Fm
            Console.WriteLine("\nPalmer algoritmus:");
            Palmer(job, numberJob, numberRes, s);
            Simulation(job, numberJob, res, numberRes, s, 0, cut_mode);
            Console.WriteLine("Cmax értéke:{0}", job[s[numberJob - 1]].endT[numberRes - 1]);

            //Dannenbring - Fm
            Console.WriteLine("\nDannenbring algoritmus:");
            Dannenbring_alg(job, numberJob, numberRes, s);
            Simulation(job, numberJob, res, numberRes, s, 0, cut_mode);
            Console.WriteLine("Cmax értéke:{0}", job[s[numberJob - 1]].endT[numberRes - 1]);

            //CDS algoritmus - Fm 
            Console.WriteLine("\nCDS algoritmus:");
            CDS_alg(job, numberJob, res, numberRes, s, cut_mode);
            Simulation(job, numberJob, res, numberRes, s, 0, cut_mode);
            Console.WriteLine("Cmax értéke:{0}", job[s[numberJob - 1]].endT[numberRes - 1]);

            //Result
            Result(job,numberJob, numberRes,s,f);
            Console.WriteLine("\n\n Celfuggvenyek ertekei:");

            for (int k = 0; k < f.Length; k++)
            {
                Console.WriteLine("\n f[{0}] = {1}", k, f[k]);
            }
                
        }

        public static void Simulation(Job[] job, int numberJob, Resourse[] resourse, int numberRes, int[] s, int t0, int cut_mode)
        {
            for (int i = 0; i < numberJob; i++)
            {
                for (int r = 0; r < numberRes; r++)
                {
                    if (i == 0) //legelso munka
                    {
                        if (r == 0)//legelso gepen
                        {
                            job[s[i]].startT[r] = t0;
                        }
                        else //nem a legelso gepen
                        {
                            job[s[i]].startT[r] = job[s[i]].endT[r - 1] + resourse[r].transT[r - 1]; //előző befejézési ideje plusz a mozgatas
                        }
                        //megállapítjuk a befejezési időt: indul+dolgozik+átáll
                        job[s[i]].endT[r] = job[s[i]].startT[r] + resourse[r].setT[0,s[i]] + job[s[i]].procT[r];
                    }
                    else//nem a legelso munka
                    {
                        if (r == 0)//legelso gepen
                        {
                            job[s[i]].startT[r] = job[s[i - 1]].endT[r];
                        }
                        else//nem a legelso gepen
                        {
                            job[s[i]].startT[r] = Math.Max(job[s[i]].endT[r - 1] + resourse[r].transT[r - 1], job[s[i - 1]].endT[r]); //előző gépről mikor indul és elozo munka mikor fejeződik be
                        }
                        //megállapítjuk a befejezési időt: indul+dolgozik+átáll
                        job[s[i]].endT[r] = job[s[i]].startT[r] + resourse[r].setT[s[i-1],s[i]] + job[s[i]].procT[r];
                    }
                    //eroforrashoz illesztes
                    //!!!
                    int vmi = cut_mode;
                }

            }
        }

        //F2|perm|Cmax 
        //U={1,2,3,4}
        //S={?,?,?,?}
        public static void Johnson_alg(Job[] job, int numberJob, int r, int[] s)
        {
            int index, temp;
            int value, val_of_j;
            int[] u = new int[numberJob]; //előrendezés
            int first, last; //szabad helyek indexe

            for (int i = 0; i < numberJob; i++)
            {
                u[i] = i; //kiindulás, utemezni kívánt munkák id-je
            }

            //előrendezés
            for (int i = 0; i < numberJob - 1; i++) // így megy végig az U tömbön //függőlegesen mn, jobok közti min
            {
                index = i;
                value = Math.Min(job[u[i]].procT[r], job[u[i]].procT[r + 1]);
                for (int j = i + 1; j < numberJob; j++) //1 sorban mi a minimum
                {
                    val_of_j = Math.Min(job[u[j]].procT[r], job[u[j]].procT[r + 1]);
                    if (val_of_j < value)
                    {
                        index = j;
                        value = val_of_j;
                    }
                }
                if (index != i) //van jobb jelolt
                {  //csere
                    temp = u[index];
                    u[index] = u[i];
                    u[i] = temp;
                }
            }

            //ütemezés
            first = 0;    //eleje
            last = numberJob - 1;  //vege

            for (int i = 0; i < numberJob; i++)
            {
                if (job[u[i]].procT[r] <= job[u[i]].procT[r + 1])
                { //elölről nézve az elso szabad helyre
                    s[first] = u[i];
                    first++;
                }
                else
                { //hatulsol az elso szabad helyre
                    s[last] = u[i];
                    last--;
                }
            }
        }

        public static void Johnson3_alg(Job[] job, int numberJob, int r, int[] s)
        {
            Job[] virt = new Job[numberJob];
            int min_0, max_1, min_2;

            for (int i = 0; i < numberJob; i++)
            {
                virt[i].procT = new int[2];
                virt[i].procT[0] = job[i].procT[r] + job[i].procT[r + 1];
                virt[i].procT[1] = job[i].procT[r + 1] + job[i].procT[r + 2];
            }

            Johnson_alg(virt, numberJob, 0, s);

            //optimalitás vizsgálat
            min_0 = job[0].procT[0];
            max_1 = job[0].procT[1];
            min_2 = job[0].procT[2];

            for (int i = 0; i < numberJob; i++)
            {
                if (min_0 > job[i].procT[0])
                    min_0 = job[i].procT[0];

                if (max_1 < job[i].procT[1])
                    max_1 = job[i].procT[1];

                if (min_2 > job[i].procT[2])
                    min_2 = job[i].procT[2];
            }
            if (min_0 <= max_1 ||
        min_2 <= max_1
      )
                Console.WriteLine("Optimális.");
            else
                Console.WriteLine("Nem biztos, hogy optimális.");
        }

        public static void Palmer(Job[] job, int numberJob, int numberRes, int[] s)
        {
            List<PalmerJob> palmerPriority = new List<PalmerJob>(); //prioritas

            foreach (var j in job)
            {
                PalmerJob palmerJob = new PalmerJob(j.id, 0);
                palmerPriority.Add(palmerJob);
            }


            for (int i = 0; i < numberJob; i++)
            {
                //I[i] = 0;
                for (int j = 0; j < numberRes; j++)
                {
                    palmerPriority[i].p += -1 * job[i].procT[j] * (numberRes - (2 * (j + 1) - 1)) / 2;

                }
            }
            for (int i = 0; i < numberJob; i++)
            {
                s[i] = i;
            }
            foreach (var item in palmerPriority)
            {
                Console.WriteLine("elotte. "+ item.p);
            }
            Console.WriteLine("-----------------------------------------------------");
            //Ütemezés prioritás alapján
            palmerPriority.Sort((x, y) => x.p.CompareTo(y.p));
            
            palmerPriority.Reverse();
            foreach (var item in palmerPriority)
            {
                Console.WriteLine("asdfg" + item.p);
            }
            for (int i = 0; i < palmerPriority.Count; i++)
            {
                s[i] = palmerPriority[i].Id;
            }
        }


        //F|perm|Cmax esetén a célfüggvény, késés, csuszas stb. értéke
        public static void Result(Job[] job, int numberJob, int numberRes, int[] s, double[] f)
        {
            int C; //bef.idopont
            int L; //keses
            int T; //csuszas

            double Tmax = 0; //max csuszás
            double Tsum = 0; //csúszás összeg
            double Usum = 0; //késő munkak szama

            for (int i = 0; i < numberJob; i++)
            {
                //adott munka, utolsó erőforráson a bef.ideje
                C = job[i].endT[numberRes - 1];
                //bef.ido - határidő
                L = C - job[i].dueD;
                //maximum a 0, keses
                T = Math.Max(0, L);
                if (i == 0)
                    Tmax = T;
                else
                if (Tmax < T)
                    Tmax = T;

                Tsum += T;

                if (T > 0)
                    Usum++;

                //célfgvek
                f[0] = job[s[numberJob - 1]].endT[numberRes - 1];  //Cmax: F|perm|Cmax eseteben
                f[1] = Tmax;
                f[2] = Tsum;
                f[3] = Usum;
            }
        }

        //Fm|perm|Cmax
        public static void Dannenbring_alg(Job[] job, int numberJob, int numberRes, int[] s)
        {
            Job[] virt = new Job[numberJob];
            for (int i = 0; i < numberJob; i++)
            {
                virt[i].procT = new int[2];
                virt[i].procT[0] = 0;
                virt[i].procT[1] = 0;

                for (int j = 0; j < numberRes; j++)
                {
                    virt[i].procT[0] += job[i].procT[j] * (numberRes - (j + 1) + 1);
                    virt[i].procT[1] += job[i].procT[j] * (j + 1);
                }
            }
            Johnson_alg(virt, numberJob, 0, s);
        }

        //Fm|perm|Cmax
        public static void CDS_alg(Job[] job, int numberJob,Resourse[]res, int numberRes, int[] s, int cut_mode)
        {
            int[] actual_sol = new int[numberJob]; //aktuális
            int[] best_sol = new int[numberJob]; //legjobb

            int actual_C; //completion time
            int best_C = 0;

            for (int i = 0; i < numberJob; i++)
            {
                //virtuális kétgépes feladat Johnsonnal
                Johnson_alg(job, numberJob, i, actual_sol);
                //szimu
                Simulation(job, numberJob, res, numberRes, actual_sol, 0, cut_mode);
                //kiertekeles
                actual_C = job[s[numberJob - 1]].endT[numberRes - 1];

                if (i == 0)
                {
                    best_C = actual_C;
                    set_S_to_S2(best_sol, actual_sol, numberJob);
                }
                else
                {
                    if (best_C > actual_C)
                    {
                        best_C = actual_C;
                        set_S_to_S2(best_sol, actual_sol, numberJob);
                    }
                }
            }
        }

        public static void set_S_to_S2(int[] s1, int[] s2, int numberJob)
        {
            for (int j = 0; j < numberJob; j++)
            {
                s1[j] = s2[j];
            }
        }

        public static void Print_Res_Interval(Resourse[] res, int numberRes)
        {
            int r;
            int c;

            Console.WriteLine("\n\n Eroforrasok rendelkezesre allasi idointervallumai");
            for (r = 0; r < numberRes; r++)
            {
                Console.WriteLine("\n %d. eroforras [%d]", r, res[r].intervalN);
                Console.WriteLine("\n # \t Kezdet\tVege");
                for (c = 0; c < res[r].intervalN; c++)
                {
                    Console.WriteLine("\n %d \t %ld \t %ld", c, res[r].interval[c].ST, res[r].interval[c].ET);
                }

            }
        }

        public static int Set_op_to_TW(int st, int et, Resourse[] res, int r)
        {//visszaadja melyik TW a megfelelő
            //azt a TimeWindow-t keresi, ahova az adott operációt el tudja helyezni megszakítás nélkül

            int modified_st = st;
            int modified_et = et;
            int execution_time = et - st; //végrehajtási idő

            int f = -1;
            int c = 0; //hanyadik TW

            while (c < res[r].intervalN)
            {
                if (modified_st < res[r].interval[c].ET)// ez esetben tudjuk csak vizsgálni
                {
                    //hatarra illesztes ha kell
                    modified_st = Math.Max(modified_st, res[r].interval[c].ST);
                    modified_et = modified_st + execution_time;
                    if (modified_et <= res[r].interval[c].ET)
                    {//belefer
                        f = c;
                        break;
                    }
                    else
                    {//kilog
                        c++;
                        if (c >= res[r].intervalN)
                        {
                            modified_st = res[r].interval[c - 1].ET;
                            modified_et = modified_st + execution_time;
                            break;
                        }
                        continue;
                    }

                }
                c++;
            }
            st = modified_st;
            et = modified_et;
            return f;
        }

        public static int Set_op_to_TW_w_cut(int st, int et, Resourse[] res, int r)
        {//visszaadja melyik TW a megfelelő
            //azt a TimeWindow-t keresi, ahova az adott operációt el tudja helyezni megszakítással
            int modified_st = st;
            int modified_et = et;
            int execution_time = et - st; //végrehajtási idő

            int f = -1;
            int c = 0; //hanyadik TW
            int fps = -1; //elso resz kezdete

            while (c < res[r].intervalN)
            {
                if (modified_st < res[r].interval[c].ET)// ez esetben tudjuk csak vizsgálni
                {
                    //hatarra illesztes ha kell
                    modified_st = Math.Max(modified_st, res[r].interval[c].ST);
                    modified_et = modified_st + execution_time;

                    if (fps == -1)
                        fps = modified_st;  //elso darab inditasa

                    if (modified_et <= res[r].interval[c].ET)
                    {//belefer
                        f = c;
                        break;
                    }
                    else
                    {//kilog
                        c++;
                        if (c >= res[r].intervalN)
                        {
                            modified_et = modified_st + execution_time;
                            break;
                        }
                        execution_time -= res[r].interval[c - 1].ET - modified_st;
                        continue;
                    }

                }
                c++;
            }
            if (fps != -1)
                st = fps;
            else
                st = modified_st;

            et = modified_et;
            return f;
        }

        public static int SOTTW_mode(int st, int et, Resourse[] res, int r, int cut_mode)
        {
            int return_value;
            if (cut_mode == 1)
            {
                return_value = Set_op_to_TW_w_cut(st, et, res, r);
            }
            else
            {
                return_value = Set_op_to_TW(st, et, res, r);
            }
            return return_value;
        }

        //relatív változások súlyozott összege
        public static double F(int[] fx, int[] fy, int[] w, int K)
        //2 célfgvek tömbje, prioritásokat tartalmazó vektor, célfgv száma
        {
            double F = 0;
            double a, b;
            double D;

            for (int k = 0; k < K; k++)
            {
                a = fx[k];
                b = fy[k];
                //D (distance) értékének meghatározása
                if (Math.Max(a, b) == 0)
                {
                    D = 0;
                }
                else
                {
                    D = (b - a) / Math.Max(a, b);
                }

                //Összeszummázzuk, adott prioritással
                F += w[k] * D;
            }
            return F;
        }



    }
}
