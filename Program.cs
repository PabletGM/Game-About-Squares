//Pablo García Moreno
//Beatriz Rubio Rodríguez
using System;
using System.IO;

namespace Main
{
    class MainClass
    {
        const char // Flechas y símbolos en utf8
            upA = '▲', downA = '▼', leftA = '◄', rightA = '►',
            goal = '●', free = '■',
            upG = '^', downG = 'v', rightG = '>', leftG = '<';
        // tamaño de la cuadrícula
        const int DIM = 12;
        //numero maximo de niveles
        const int maxNivel = 42;
        // posicion del numero del nivel
        const int posNumNiv = 3;
        //posicion del numero de movimientos
        const int posNumMov = 11;
        // dirección de bloques y giros
        enum Direccion { Arriba, Abajo, Izquierda, Derecha, None };
        struct Coordenada
        { // posiciones en la cuadrícula
            public int x, y;
        }
        struct Bloque
        {
            public Coordenada pos;
            public int color;
            public Direccion dir;
        }
        struct Estado
        { // estado del juego
            public int[,] huecos;
            public Direccion[,] giros;
            public Bloque[] bloques;
            public Coordenada cursor;
        }
        public static void Main()
        {   //Numero del nivel lo definimos como n
            int n = 1;
            int NumMovimientos = 0;
            int[] Movimientos = new int[maxNivel];
            bool juego = true;
            //Caracteres especiales
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            char c = ' ';
            Estado est = new Estado();
            Console.Write("¿Quieres cargar una partida?: [s/n]");
            string respuesta = Console.ReadLine();
            //Si el jugador desea seguir jugando desde el último nivel completado
            if (respuesta == "s")
            {
                Console.Write("Nombre de la partida: ");
                string file = Console.ReadLine();
                n = Sobreescribir(file, Movimientos);
            }
            //Render inicial
            LeeNivel(n, out est);
            Render(est);
            //Bucle principal de juego
            while (juego)
            {
                NumMovimientos = 0;
                //Juego
                while (!FinNivel(est) && juego)
                {
                    ProcesaInput(LeeInput(), ref est, ref NumMovimientos, n, ref juego);
                }
                //Cuando acaba el nivel, que se inicie el siguiente
                if (FinNivel(est))
                {   //Guardar el numero de movimientos del nivel acabado
                    GuardarMovimientos(Movimientos, NumMovimientos, n);
                    n++;
                    //Render del siguiente nivel
                    LeeNivel(n, out est);
                    Render(est);
                }
                //Si se llega al último nivel (42), se acaba el juego
                if (n == maxNivel)
                {
                    juego = false;
                }
            }
            Console.Clear();
            //Gestión de usuarios
            Console.Write("¿Quieres guardar la partida? [s/n]");
            string sn = Console.ReadLine();
            if (sn == "s")
            {
                Console.Write("Nombre del archivo: ");
                string archivo = Console.ReadLine();
                //Si existe un archivo con ese nombre, se guarda. Si no, se crea el archivo y se guarda.
                if (File.Exists(archivo))
                {
                    Guardar(est, ref n, Movimientos, NumMovimientos, sn, archivo);
                }

                else
                {
                    CrearArchivo(archivo);
                    Guardar(est, ref n, Movimientos, NumMovimientos, sn, archivo);
                }
            }
            //Para ocultar las letras de abajo
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine();
        }
        static string LeeNivel(int n, out Estado est)
        {   //Inicialización de Estado
            est.cursor.x = 0; est.cursor.y = 0;
            est.huecos = new int[DIM, DIM]; est.bloques = new Bloque[DIM + 1];
            est.giros = new Direccion[DIM, DIM];

            string levels = "levels.txt";
            StreamReader f = new StreamReader(levels);
            string NombreNivel = " ";
            string[] s;

            bool nivel = false;
            //Definimos por defecto huecos =- 1
            for (int i = 0; i < DIM; i++)
            {
                for (int j = 0; j < DIM; j++)
                {
                    est.huecos[i, j] = -1;
                }
            }
            //Definimos por defecto todos los giros en direccion NONE
            for (int i = 0; i < DIM; i++)
            {
                for (int j = 0; j < DIM; j++)
                {
                    est.giros[i, j] = Direccion.None;
                }
            }
            //Definimos por defecto todos los bloques en direccion NONE
            for (int i = 0; i < DIM + 1; i++)
            {
                est.bloques[i].dir = Direccion.None;
            }
            //Definimos por defecto todos los bloques en color = -1 
            for (int i = 0; i < DIM; i++)
            {
                est.bloques[i].color = -1;

            }
            //Dividimos cada linea en palabras
            string linea = f.ReadLine();
            s = linea.Split(' ');

            while (!nivel && !f.EndOfStream)
            {
                if (n > 1)
                {
                    linea = f.ReadLine();
                }

                s = linea.Split(' ');

                if (s[0] == "level" && s[1] == n.ToString())
                {
                    nivel = true;
                    NombreNivel = s[2];
                }
            }

            if (!nivel)
            {
                Console.Write("Error");
            }

            int numeroBloques = 0;
            while (s[0] != "")
            {   //Se lee la linea y se divide en palabras
                linea = f.ReadLine();
                s = linea.Split(' ');

                if (s[0] == "hueco")
                {   //Si se encuenta con la palabra "hueco", asignamos pos y valor
                    int valor1 = int.Parse(s[1]);
                    int valor2 = int.Parse(s[2]);
                    int valor4 = int.Parse(s[4]);
                    est.huecos[valor1, valor2] = valor4;


                }
                else if (s[0] == "bloque")
                {   //Si se encuentra con la palabra "bloque", asignamos dir, color y pos
                    int valor1 = int.Parse(s[1]);
                    int valor2 = int.Parse(s[2]);
                    int valor4 = int.Parse(s[4]);
                    string valor5 = linea.Split(' ')[5];

                    est.bloques[numeroBloques].pos.x = valor1;
                    est.bloques[numeroBloques].pos.y = valor2;
                    est.bloques[numeroBloques].color = valor4;

                    if (valor5 == "up")
                    { est.bloques[numeroBloques].dir = Direccion.Arriba; }
                    else if (valor5 == "")
                    { est.bloques[numeroBloques].dir = Direccion.None; }
                    else if (valor5 == "down")
                    { est.bloques[numeroBloques].dir = Direccion.Abajo; }
                    else if (valor5 == "right")
                    { est.bloques[numeroBloques].dir = Direccion.Derecha; }
                    else
                    { est.bloques[numeroBloques].dir = Direccion.Izquierda; }

                    numeroBloques++;
                }
                else if (s[0] == "giro")
                {   //Si se encuentra con la palabra "giro", asignamos pos y dir
                    int valor1 = int.Parse(s[1]);
                    int valor2 = int.Parse(s[2]);
                    string valor5 = linea.Split(' ')[3];

                    if (valor5 == "up")
                    { est.giros[valor1, valor2] = Direccion.Arriba; }
                    else if (valor5 == "")
                    { est.giros[valor1, valor2] = Direccion.None; }
                    else if (valor5 == "down")
                    { est.giros[valor1, valor2] = Direccion.Abajo; }
                    else if (valor5 == "right")
                    { est.giros[valor1, valor2] = Direccion.Derecha; }
                    else
                    { est.giros[valor1, valor2] = Direccion.Izquierda; }
                }
            }
            return NombreNivel;
        }
        #region Render
        static void Render(Estado est)
        {
            //Colores para los huecos
            ConsoleColor[] cols = {
            ConsoleColor.Red,ConsoleColor.Blue,ConsoleColor.Green,ConsoleColor.Magenta,
            ConsoleColor.Yellow,ConsoleColor.Cyan,ConsoleColor.Gray};
            //Colores para los bloques
            ConsoleColor[] colsDark = {
            ConsoleColor.DarkRed,ConsoleColor.DarkBlue,ConsoleColor.DarkGreen,
            ConsoleColor.DarkMagenta,ConsoleColor.DarkYellow,ConsoleColor.DarkCyan,
            ConsoleColor.DarkGray};

            Console.Clear();
            //Rellenamos la matriz con casillas vacías
            RenderVacio(est, free);
            //Dibujo de matriz de huecos
            RenderHuecos(est, cols, goal);
            //Dibujo de matriz de giros
            RenderGiros(est, upG, downG, rightG, leftG);
            //Dibujo de matriz de bloques
            RenderBloques(est, est.bloques[DIM], colsDark, cols, upA, downA, rightA, leftA);
            //Poner el cursor en la posición donde te muevas segun ProcesaInput
            Console.SetCursorPosition(2 * est.cursor.x, est.cursor.y);
        }
        static void ExcepcionBloqueHueco(int numerodeBloque, int x, int y, Estado est, ConsoleColor[] cols, ConsoleColor[] colsDark)
        {   //Si coincide bloque+hueco se renderiza de una manera diferente
            Console.SetCursorPosition(2 * x, y);
            Console.BackgroundColor = cols[est.huecos[x, y]];
            Console.ForegroundColor = colsDark[est.bloques[numerodeBloque].color];
            //Según la direccion renderizamos un icono o otro
            if (est.bloques[numerodeBloque].dir == Direccion.Arriba) Console.Write(upA);
            else if (est.bloques[numerodeBloque].dir == Direccion.Abajo) Console.Write(downA);
            else if (est.bloques[numerodeBloque].dir == Direccion.Derecha) Console.Write(rightA);
            else if (est.bloques[numerodeBloque].dir == Direccion.Izquierda) Console.Write(leftA);
            //Para quitar el  color por defecto de fondo y foreground
            Console.ResetColor();
        }
        static void RenderHuecos(Estado est, ConsoleColor[] cols, char goal)
        {
            for (int i = 0; i < DIM; i++)
            {
                for (int j = 0; j < DIM; j++)
                {   //En caso de que el hueco exista, se colorea.
                    if (est.huecos[i, j] != -1)
                    {
                        Console.SetCursorPosition(2 * i, j);
                        if (est.huecos[i, j] == 0) { Console.ForegroundColor = cols[0]; }
                        else if (est.huecos[i, j] == 1) { Console.ForegroundColor = cols[1]; }
                        else if (est.huecos[i, j] == 2) { Console.ForegroundColor = cols[2]; }
                        else if (est.huecos[i, j] == 3) { Console.ForegroundColor = cols[3]; }
                        else if (est.huecos[i, j] == 4) { Console.ForegroundColor = cols[4]; }
                        else if (est.huecos[i, j] == 5) { Console.ForegroundColor = cols[5]; }
                        else { Console.ForegroundColor = cols[6]; }
                        Console.Write(goal);
                    }
                    Console.ResetColor();
                }
            }
        }
        static void RenderGiros(Estado est, char upG, char downG, char rightG, char leftG)
        {
            for (int i = 0; i < DIM; i++)
            {
                for (int j = 0; j < DIM; j++)
                {
                    Console.SetCursorPosition(2 * i, j);

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    //Según su dirección, se renderiza de una manera u otra.
                    if (est.giros[i, j] == Direccion.Arriba) Console.Write(upG);
                    else if (est.giros[i, j] == Direccion.Abajo) Console.Write(downG);
                    else if (est.giros[i, j] == Direccion.Derecha) Console.Write(rightG);
                    else if (est.giros[i, j] == Direccion.Izquierda) Console.Write(leftG);
                }
            }
        }
        static void RenderBloques(Estado est, Bloque bloque, ConsoleColor[] colsDark, ConsoleColor[] cols, char upA, char downA, char rightA, char leftA)
        {
            for (int i = 0; i < DIM; i++)
            {   //Si existe el bloque y coincide su posición con la del hueco
                if (est.bloques[i].dir != Direccion.None && est.huecos[est.bloques[i].pos.x, est.bloques[i].pos.y] != -1)
                {   //Se renderiza de una manera especial porque es una excepción
                    ExcepcionBloqueHueco(i, est.bloques[i].pos.x, est.bloques[i].pos.y, est, cols, colsDark);
                }
                else
                {
                    if (est.bloques[i].color != -1)
                    {
                        Console.SetCursorPosition(2 * est.bloques[i].pos.x, est.bloques[i].pos.y);
                        Console.ForegroundColor = colsDark[est.bloques[i].color];
                        //Si coincide la posición del bloque y del giro
                        if (est.giros[est.bloques[i].pos.x, est.bloques[i].pos.y] != Direccion.None) AplicaGiro(est.giros, ref est.bloques[i]);
                        //Si no coincide con nada
                        if (est.bloques[i].dir == Direccion.Arriba) Console.Write(upA);
                        else if (est.bloques[i].dir == Direccion.Abajo) Console.Write(downA);
                        else if (est.bloques[i].dir == Direccion.Derecha) Console.Write(rightA);
                        else if (est.bloques[i].dir == Direccion.Izquierda) Console.Write(leftA);
                    }
                }
            }
        }
        static void RenderVacio(Estado est, char free)
        {   //Renderizamos todas las casillas vacías 
            for (int i = 0; i < DIM; i++)
            {
                for (int j = 0; j < DIM; j++)
                {
                    Console.SetCursorPosition(2 * i, j);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(free);
                }
                Console.WriteLine();
            }
        }
        #endregion 
        static void ProcesaInput(char c, ref Estado est, ref int NumMovimientos, int NumeroNivel, ref bool juego)//
        {
            switch (c)
            {   //Teclas de desplazamiento de cursor
                case 'u':
                    if (est.cursor.y > 0) est.cursor.y -= 1; Render(est);
                    break;
                case 'd':
                    if (est.cursor.y < DIM - 1) est.cursor.y += 1; Render(est);
                    break;
                case 'l':
                    if (est.cursor.x > 0) est.cursor.x -= 1; Render(est);
                    break;
                case 'r':
                    if (est.cursor.x < DIM - 1) est.cursor.x += 1; Render(est);
                    break;
                //Click
                case 'e':
                    ClickCasilla(ref est, ref NumMovimientos); Render(est);
                    break;
                //Reseteo del nivel
                case 'X':
                    Console.Clear(); LeeNivel(NumeroNivel, out est); Render(est);
                    break;
                //Salir del juego
                case 'q':
                    juego = !juego;
                    break;
                default:
                    break;
            }
        }
        static bool Dentro(Coordenada c)
        {   //Comprueba si la coordenada está dentro del tablero.
            if (c.x >= 0 && c.x < DIM && c.y >= 0 && c.y < DIM)
            {
                return true;
            }
            else return false;
        }
        static void AplicaGiro(Direccion[,] giros, ref Bloque b)
        {   //Si el bloque está en una casilla de giro, le aplica dicho giro
            if (giros[b.pos.x, b.pos.y] != Direccion.None)
            {
                b.dir = giros[b.pos.x, b.pos.y];
            }
        }
        static int BuscaBloque(Coordenada c, Bloque[] bloques)
        {   //Devuelve el valor del array del bloque (si existe en esa coordenada). Si no existe, devuelve -1
            bool bloqueExistente = false;
            int i = 0;
            while (i < bloques.Length && !bloqueExistente)
            {   //Comparamos la posicion del bloque con las coordenadas c que debes buscar en caso de que exista el bloque
                if ((bloques[i].pos.x == c.x && bloques[i].pos.y == c.y) && bloques[i].dir != Direccion.None) bloqueExistente = true;
                i++;
            }
            //Ha encontrado el bloque en la coordenada así que devuelve su numero en el array 
            if (bloqueExistente)
            {
                i--;
                return i;
            }
            //No existe el bloque en esa coordenada 
            else return -1;
        }
        static Coordenada BuscaCasillaLibre(Coordenada pos, Coordenada dir, Bloque[] bloques)
        {   //Devuelve la coordenada de la casilla libre en una dirección
            int i = 1;
            bool casillaLibre = false;
            Coordenada libre;
            libre.x = pos.x; libre.y = pos.y;

            while (!casillaLibre)
            {   //Una casilla Libre es cualquiera que no posea un bloque
                if (BuscaBloque(libre, bloques) == -1) casillaLibre = true;

                else
                {
                    libre.x += dir.x; libre.y += dir.y;
                }

                i++;
            }
            return libre;
        }
        static void ClickCasilla(ref Estado est, ref int NumMovimientos)
        {   //Almacenamos el número de bloque en la variable i
            int i = BuscaBloque(est.cursor, est.bloques);
            Coordenada direccion;
            direccion.x = 0; direccion.y = 0;
            //Si existe el bloque, convertimos su dirección en una coordenada
            if (i != -1 && est.bloques[i].color != -1)
            {
                if (est.bloques[i].dir == Direccion.Arriba)
                {
                    direccion.x = 0; direccion.y = -1;
                }
                else if (est.bloques[i].dir == Direccion.Abajo)
                {
                    direccion.x = 0; direccion.y = 1;
                }
                else if (est.bloques[i].dir == Direccion.Derecha)
                {
                    direccion.x = 1; direccion.y = 0;
                }
                else
                {
                    direccion.x = -1; direccion.y = 0;
                }
            }
            if (i != -1)
            {   //La coordenada temp es un cursor que busca la casilla libre
                Coordenada temp;
                temp.x = est.cursor.x; temp.y = est.cursor.y;
                temp.x += direccion.x; temp.y += direccion.y;
                //Comprobamos si la casilla siguiente al bloque, es un bloque o una casilla libre
                int k = BuscaBloque(temp, est.bloques);
                //MOVIMIENTO BLOQUES APILADOS 
                if (k != -1)
                {   //Almacenamos la coordenada de la casilla libre y del último bloque apilado
                    Coordenada c = BuscaCasillaLibre(est.cursor, direccion, est.bloques);
                    Coordenada ultimoBloqueApilado;
                    ultimoBloqueApilado.x = c.x - direccion.x; ultimoBloqueApilado.y = c.y - direccion.y;
                    //Si ambas coordenadas están dentro del tablero
                    if (Dentro(c) && Dentro(ultimoBloqueApilado))
                    {
                        Coordenada cursorCasillaLibre = c;
                        while (cursorCasillaLibre.x != est.cursor.x || cursorCasillaLibre.y != est.cursor.y)
                        {   //Intercambiamos la casilla libre con el bloque
                            cursorCasillaLibre.x -= direccion.x; cursorCasillaLibre.y -= direccion.y;
                            int f = BuscaBloque(cursorCasillaLibre, est.bloques);
                            est.bloques[f].pos.x += direccion.x; est.bloques[f].pos.y += direccion.y;

                            //Se aplica el giro si lo hay, y sumamos 1 al contador de movimientos
                            AplicaGiro(est.giros, ref est.bloques[f]);
                            Movimientos(ref NumMovimientos);
                        }
                    }
                }
                //MOVIMIENTO DE UN SÓLO BLOQUE CON LA CASILLA LIBRE
                else
                {
                    Coordenada libre = BuscaCasillaLibre(est.cursor, direccion, est.bloques);
                    if (Dentro(libre))
                    {
                        while (libre.x != est.cursor.x || libre.y != est.cursor.y)
                        {   //Intercambiamos la casilla libre con el bloque
                            libre.x -= direccion.x; libre.y -= direccion.y;
                            est.bloques[i].pos.x += direccion.x; est.bloques[i].pos.y += direccion.y;

                            //Se aplica el giro si lo hay, y sumamos 1 al contador de movimientos
                            AplicaGiro(est.giros, ref est.bloques[i]);
                            Movimientos(ref NumMovimientos);
                        }
                        est.cursor.x += direccion.x;
                        est.cursor.y += direccion.y;
                    }
                }
            }
        }
        #region FinNivel
        static bool FinNivel(Estado est)
        {
            bool Acabar = true;
            int i = 0;
            int nBloques = NumBloques(est);

            while (i < nBloques && Acabar)
            {   //Guardamos las coordenadas del bloque en 2 variables
                int x = est.bloques[i].pos.x; int y = est.bloques[i].pos.y;

                //Verificamos si el valor de huecos en esas coordenadas vale -1 y tiene diferente color (entonces no se acaba)
                if ((est.huecos[x, y] == -1) && (est.huecos[x, y] != est.bloques[i].color))
                {
                    Acabar = false;
                }
                i++;
            }
            return Acabar;
        }
        static int NumBloques(Estado est)
        {   //Calcula el número de bloques que existen en la partida
            int nBloques = 0;

            for (int i = 0; i < DIM; i++)
            {
                if (est.bloques[i].dir != Direccion.None)
                {
                    nBloques++;
                }
            }
            return nBloques;
        }
        #endregion 
        #region Gestión de usuarios
        static int Sobreescribir(string file, int[] Movimientos)
        {   //Lee el archivo del usuario para jugar el último nivel
            StreamReader f = new StreamReader(file);
            int nivel = 1;
            string[] palabras;
            string frase;
            //Mientras que no haya una linea en blanco
            while (!f.EndOfStream)
            {   //Separamos la línea en palabras
                frase = f.ReadLine();
                palabras = frase.Split(" ");
                //Almacena la tercera palabra (número de nivel) y la onceava (número de movimientos)
                nivel = int.Parse(palabras[posNumNiv]);
                Movimientos[int.Parse(palabras[posNumNiv]) - 1] = int.Parse(palabras[posNumMov]);
            }
            f.Close();
            return nivel;
        }
        static void CrearArchivo(string file)
        {   //En caso de no existir un archivo, se crea
            StreamWriter f = new StreamWriter(file);
            f.Close();
        }
        static void Guardar(Estado est, ref int NumeroNivel, int[] Movimientos, int NumMovimientos, string NombreNivel, string file)
        {   //Guarda la última partida jugada (el número del nivel, su nombre y el número de movimientos)
            StreamWriter f = new StreamWriter(file);
            //Escribimos los datos de cada nivel
            for (int i = 0; i < NumeroNivel; i++)
            {   //Pasamos como parámetro a LeeNivel "i+1" ya que se empieza en el nivel 1, no en el 0
                NombreNivel = LeeNivel((i + 1), out est);
                f.WriteLine("Número de nivel: " + (i + 1) + " Nombre de nivel: " + NombreNivel + " Numero de novimientos: " + Movimientos[i]);
            }
            f.Close();
        }
        static void GuardarMovimientos(int[] Movimientos, int NumMovimientos, int NumeroNivel)
        {   //Guardamos el número de movimientos que haya en el nivel
            Movimientos[NumeroNivel - 1] = NumMovimientos;
        }
        static int Movimientos(ref int NumMovimeintos)
        {   //Contador de los movimientos de los bloques
            NumMovimeintos++;
            return NumMovimeintos;
        }
        #endregion 
        static char LeeInput()
        {
            char d = ' ';
            if (Console.KeyAvailable)
            {
                string tecla = Console.ReadKey().Key.ToString().ToUpper();
                switch (tecla)
                {
                    case "LEFTARROW": d = 'l'; break;
                    case "UPARROW": d = 'u'; break;
                    case "RIGHTARROW": d = 'r'; break;
                    case "DOWNARROW": d = 'd'; break;
                    case "ENTER": case "SPACEBAR": d = 'e'; break;
                    case "Escape": case "Q": d = 'q'; break;
                    case "X": d = 'X'; break;
                    case "R": d = 'R'; break;
                }
            }
            return d;
        }
    }
}