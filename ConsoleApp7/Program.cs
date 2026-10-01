namespace ConsoleApp7

    {
        class Program
        {
            
            const double Lambda = 1.0; 
            const double Mu = 1.2;    
            const int MaxBufferSize = 3;            
            const int TotalPacketsToArrive = 10;

            static Random rand = new Random();

            static double GetExponentialTime(double rate)
            {
                return -Math.Log(1.0 - rand.NextDouble()) / rate;
            }

            static void Main(string[] args)
            {
                double currentTime = 0.0;
                int packetsArrived = 0;
                int packetsDropped = 0;
                int packetsProcessed = 0;

                Queue<double> buffer = new Queue<double>();

                double processorBusyUntil = 0.0;

                Console.WriteLine("=== СТАРТ СИМУЛЯЦИИ ===");
                Console.WriteLine($"Параметры: Буфер = {MaxBufferSize}, Всего пакетов = {TotalPacketsToArrive}\n");

                double nextArrivalInterval = GetExponentialTime(Lambda);
                double nextArrivalTime = currentTime + nextArrivalInterval;

                var eventsLog = new List<(double Time, string Message)>();

                while (packetsArrived < TotalPacketsToArrive)
                {
                    packetsArrived++;
                    currentTime = nextArrivalTime;

                    while (processorBusyUntil <= currentTime && (processorBusyUntil > 0 || buffer.Count > 0))
                    {
                        if (processorBusyUntil > 0)
                        {
                            packetsProcessed++;
                            eventsLog.Add((processorBusyUntil, $"[Обработка] Пакет успешно обработан процессором. Всего обработано: {packetsProcessed}"));
                            processorBusyUntil = 0.0;
                        }

                        if (buffer.Count > 0)
                        {
                            double serviceTime = GetExponentialTime(Mu);
                            double startTime = Math.Max(processorBusyUntil, buffer.Dequeue());
                            processorBusyUntil = startTime + serviceTime;
                            eventsLog.Add((startTime, $"[Процессор] Пакет из буфера взят в обработку. Будет готов в {processorBusyUntil:F3} сек."));
                        }
                    }

                    eventsLog.Add((currentTime, $"[Приход] Пришел пакет №{packetsArrived}."));

                    if (processorBusyUntil <= currentTime && buffer.Count == 0)
                    {
                        double serviceTime = GetExponentialTime(Mu);
                        processorBusyUntil = currentTime + serviceTime;
                        eventsLog.Add((currentTime, $"[Процессор] Процессор свободен. Пакет №{packetsArrived} сразу взят в обработку. Будет готов в {processorBusyUntil:F3} сек."));
                    }
                    
                    else
                    {
                        if (buffer.Count < MaxBufferSize)
                        {
                            buffer.Enqueue(currentTime);
                            eventsLog.Add((currentTime, $"[Буфер] Процессор занят. Пакет №{packetsArrived} помещен в буфер. Мест занято: {buffer.Count}/{MaxBufferSize}"));
                        }
                        else
                        {
                            packetsDropped++;
                            eventsLog.Add((currentTime, $"[ОТКАЗ] Буфер полон ({MaxBufferSize}/{MaxBufferSize}). Пакет №{packetsArrived} ОТБРОШЕН!"));
                        }
                    }

                    nextArrivalInterval = GetExponentialTime(Lambda);
                    nextArrivalTime = currentTime + nextArrivalInterval;
                }

                while (processorBusyUntil > 0 || buffer.Count > 0)
                {
                    if (processorBusyUntil > 0)
                    {
                        packetsProcessed++;
                        eventsLog.Add((processorBusyUntil, $"[Обработка] Пакет успешно обработан процессором. Всего обработано: {packetsProcessed}"));
                        processorBusyUntil = 0.0;
                    }

                    if (buffer.Count > 0)
                    {
                        double serviceTime = GetExponentialTime(Mu);
                        double startTime = Math.Max(processorBusyUntil, buffer.Dequeue());
                        processorBusyUntil = startTime + serviceTime;
                        eventsLog.Add((startTime, $"[Процессор] Пакет из буфера взят в обработку. Будет готов в {processorBusyUntil:F3} сек."));
                    }
                }

                
                eventsLog.Sort((x, y) => x.Time.CompareTo(y.Time));


                foreach (var ev in eventsLog)
                {
                    Console.WriteLine($"[{ev.Time:F3} сек] {ev.Message}");
                }


                double dropProbability = (double)packetsDropped / TotalPacketsToArrive;

                Console.WriteLine("\n=== СТАТИСТИКА СИМУЛЯЦИИ ===");
                Console.WriteLine($"Всего поступило пакетов: {TotalPacketsToArrive}");
                Console.WriteLine($"Успешно обработано:      {packetsProcessed}");
                Console.WriteLine($"Потеряно пакетов (отказ): {packetsDropped}");
                Console.WriteLine($"Вероятность отказа:      {dropProbability:P2} ({dropProbability:F4})");
            }
        }
    }
