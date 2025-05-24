using AMAP.Business.Interfaces;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using NSubstitute;

namespace AMAP.UnitTests.Business.Mock
{
    public static class MockPaymentsInfoBc
    {
        public static IPaymentsInfoBc GetMockPaymentsInfoBc()
        {
            var mockPaymentsInfoBc = Substitute.For<IPaymentsInfoBc>();

            SetupGetCoProducerPayments(mockPaymentsInfoBc);
            SetupGetProducerPayments(mockPaymentsInfoBc);
            SetupGetProducerToReceive(mockPaymentsInfoBc);
            SetupGetCoProducerToPay(mockPaymentsInfoBc);
            SetupGetProducerTotals(mockPaymentsInfoBc);
            SetupGetCoProducerTotals(mockPaymentsInfoBc);
            SetupGetProducerCC(mockPaymentsInfoBc);
            SetupGetCoProducerCC(mockPaymentsInfoBc);

            return mockPaymentsInfoBc;
        }

        private static void SetupGetCoProducerPayments(IPaymentsInfoBc mockPaymentsInfoBc)
        {
            mockPaymentsInfoBc.GetCoProducerPayments().Returns(info =>
            {
                if (MockPaymentsInfoFlag.IsCoProducerPaymentsEmpty)
                {
                    return new Dictionary<string, Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>>();
                }
                else
                {
                    // Normal case with co-producer payments
                    return new Dictionary<string, Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>>
                    {
                        {
                            "coproducer-1", new Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>(
                                new AppUser { Id = "coproducer-1", Name = "Co-Producer 1" },
                                new Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>
                                {
                                    {
                                        1, new Tuple<Subscription, IEnumerable<Payment>>(
                                            new Subscription
                                            {
                                                Id = 1,
                                                CoProducerId = "coproducer-1",
                                                ProductOfferId = 101,
                                                ExpectedQuantity = 5,
                                                PaymentType = PaymentType.Money
                                            },
                                            new List<Payment>
                                            {
                                                new Payment
                                                {
                                                    Id = 1,
                                                    SubscriptionId = 1,
                                                    AmountPaid = 50.0,
                                                    PaymentDate = DateTime.Now.AddDays(-5)
                                                },
                                                new Payment
                                                {
                                                    Id = 2,
                                                    SubscriptionId = 1,
                                                    AmountPaid = 25.0,
                                                    PaymentDate = DateTime.Now.AddDays(-2)
                                                }
                                            }
                                        )
                                    },
                                    {
                                        2, new Tuple<Subscription, IEnumerable<Payment>>(
                                            new Subscription
                                            {
                                                Id = 2,
                                                CoProducerId = "coproducer-1",
                                                ProductOfferId = 102,
                                                ExpectedQuantity = 3,
                                                PaymentType = PaymentType.BankTransfer
                                            },
                                            new List<Payment>
                                            {
                                                new Payment
                                                {
                                                    Id = 3,
                                                    SubscriptionId = 2,
                                                    AmountPaid = 60.0,
                                                    PaymentDate = DateTime.Now.AddDays(-3)
                                                }
                                            }
                                        )
                                    }
                                }
                            )
                        },
                        {
                            "coproducer-2", new Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>(
                                new AppUser { Id = "coproducer-2", Name = "Co-Producer 2" },
                                new Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>
                                {
                                    {
                                        3, new Tuple<Subscription, IEnumerable<Payment>>(
                                            new Subscription
                                            {
                                                Id = 3,
                                                CoProducerId = "coproducer-2",
                                                ProductOfferId = 103,
                                                ExpectedQuantity = 2,
                                                PaymentType = PaymentType.MBway
                                            },
                                            new List<Payment>
                                            {
                                                new Payment
                                                {
                                                    Id = 4,
                                                    SubscriptionId = 3,
                                                    AmountPaid = 40.0,
                                                    PaymentDate = DateTime.Now.AddDays(-1)
                                                }
                                            }
                                        )
                                    }
                                }
                            )
                        }
                    };
                }
            });
        }

        private static void SetupGetProducerPayments(IPaymentsInfoBc mockPaymentsInfoBc)
        {
            mockPaymentsInfoBc.GetProducerPayments().Returns(info =>
            {
                if (MockPaymentsInfoFlag.IsProducerPaymentsEmpty)
                {
                    return new Dictionary<string, Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>>();
                }
                else
                {
                    // Normal case with producer payments
                    return new Dictionary<string, Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>>
                    {
                        {
                            "producer-1", new Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>(
                                new AppUser { Id = "producer-1", Name = "Producer 1" },
                                new Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>
                                {
                                    {
                                        1, new Tuple<Subscription, IEnumerable<Payment>>(
                                            new Subscription
                                            {
                                                Id = 1,
                                                CoProducerId = "coproducer-1",
                                                ProductOfferId = 101,
                                                ExpectedQuantity = 5,
                                                PaymentType = PaymentType.Money
                                            },
                                            new List<Payment>
                                            {
                                                new Payment
                                                {
                                                    Id = 1,
                                                    SubscriptionId = 1,
                                                    AmountPaid = 50.0,
                                                    PaymentDate = DateTime.Now.AddDays(-5)
                                                }
                                            }
                                        )
                                    }
                                }
                            )
                        },
                        {
                            "producer-2", new Tuple<AppUser, Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>>(
                                new AppUser { Id = "producer-2", Name = "Producer 2" },
                                new Dictionary<int, Tuple<Subscription, IEnumerable<Payment>>>
                                {
                                    {
                                        2, new Tuple<Subscription, IEnumerable<Payment>>(
                                            new Subscription
                                            {
                                                Id = 2,
                                                CoProducerId = "coproducer-1",
                                                ProductOfferId = 102,
                                                ExpectedQuantity = 3,
                                                PaymentType = PaymentType.BankTransfer
                                            },
                                            new List<Payment>
                                            {
                                                new Payment
                                                {
                                                    Id = 3,
                                                    SubscriptionId = 2,
                                                    AmountPaid = 60.0,
                                                    PaymentDate = DateTime.Now.AddDays(-3)
                                                }
                                            }
                                        )
                                    }
                                }
                            )
                        }
                    };
                }
            });
        }

        private static void SetupGetProducerToReceive(IPaymentsInfoBc mockPaymentsInfoBc)
        {
            mockPaymentsInfoBc.GetProducerToReceive().Returns(info =>
            {
                if (MockPaymentsInfoFlag.IsProducerToReceiveEmpty)
                {
                    return new Dictionary<string, double>();
                }
                else
                {
                    // Normal case with producer amounts to receive
                    return new Dictionary<string, double>
                    {
                        { "producer-1", 75.50 },
                        { "producer-2", 120.25 }
                    };
                }
            });
        }

        private static void SetupGetCoProducerToPay(IPaymentsInfoBc mockPaymentsInfoBc)
        {
            mockPaymentsInfoBc.GetCoProducerToPay().Returns(info =>
            {
                if (MockPaymentsInfoFlag.IsCoProducerToPayEmpty)
                {
                    return new Dictionary<string, double>();
                }
                else
                {
                    // Normal case with co-producer amounts to pay
                    return new Dictionary<string, double>
                    {
                        { "coproducer-1", 45.75 },
                        { "coproducer-2", 60.25 }
                    };
                }
            });
        }

        private static void SetupGetProducerTotals(IPaymentsInfoBc mockPaymentsInfoBc)
        {
            mockPaymentsInfoBc.GetProducerTotals().Returns(info =>
            {
                if (MockPaymentsInfoFlag.IsProducerTotalsEmpty)
                {
                    return new Dictionary<string, Tuple<double, double>>();
                }
                else
                {
                    // Normal case with producer totals
                    return new Dictionary<string, Tuple<double, double>>
                    {
                        { "producer-1", new Tuple<double, double>(100.0, 75.50) },
                        { "producer-2", new Tuple<double, double>(150.0, 120.25) }
                    };
                }
            });
        }

        private static void SetupGetCoProducerTotals(IPaymentsInfoBc mockPaymentsInfoBc)
        {
            mockPaymentsInfoBc.GetCoProducerTotals().Returns(info =>
            {
                if (MockPaymentsInfoFlag.IsCoProducerTotalsEmpty)
                {
                    return new Dictionary<string, Tuple<double, double>>();
                }
                else
                {
                    // Normal case with co-producer totals
                    return new Dictionary<string, Tuple<double, double>>
                    {
                        { "coproducer-1", new Tuple<double, double>(150.0, 135.0) },
                        { "coproducer-2", new Tuple<double, double>(80.0, 40.0) }
                    };
                }
            });
        }

        private static void SetupGetProducerCC(IPaymentsInfoBc mockPaymentsInfoBc)
        {
            mockPaymentsInfoBc.GetProducerCC().Returns(info =>
            {
                if (MockPaymentsInfoFlag.IsProducerCCEmpty)
                {
                    return new Dictionary<string, Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>>();
                }
                else
                {
                    // Normal case with producer current account
                    return new Dictionary<string, Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>>
                    {
                        {
                            "producer-1", new Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>(
                                new AppUser { Id = "producer-1", Name = "Producer 1" },
                                new List<Subscription>
                                {
                                    new Subscription
                                    {
                                        Id = 1,
                                        CoProducerId = "coproducer-1",
                                        ProductOfferId = 101,
                                        ExpectedQuantity = 5,
                                        PaymentType = PaymentType.Money
                                    }
                                },
                                new List<Payment>
                                {
                                    new Payment
                                    {
                                        Id = 1,
                                        SubscriptionId = 1,
                                        AmountPaid = 50.0,
                                        PaymentDate = DateTime.Now.AddDays(-5)
                                    }
                                }
                            )
                        },
                        {
                            "producer-2", new Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>(
                                new AppUser { Id = "producer-2", Name = "Producer 2" },
                                new List<Subscription>
                                {
                                    new Subscription
                                    {
                                        Id = 2,
                                        CoProducerId = "coproducer-1",
                                        ProductOfferId = 102,
                                        ExpectedQuantity = 3,
                                        PaymentType = PaymentType.BankTransfer
                                    }
                                },
                                new List<Payment>
                                {
                                    new Payment
                                    {
                                        Id = 3,
                                        SubscriptionId = 2,
                                        AmountPaid = 60.0,
                                        PaymentDate = DateTime.Now.AddDays(-3)
                                    }
                                }
                            )
                        }
                    };
                }
            });
        }

        private static void SetupGetCoProducerCC(IPaymentsInfoBc mockPaymentsInfoBc)
        {
            mockPaymentsInfoBc.GetCoProducerCC().Returns(info =>
            {
                if (MockPaymentsInfoFlag.IsCoProducerCCEmpty)
                {
                    return new Dictionary<string, Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>>();
                }
                else
                {
                    // Normal case with co-producer current account
                    return new Dictionary<string, Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>>
                    {
                        {
                            "coproducer-1", new Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>(
                                new AppUser { Id = "coproducer-1", Name = "Co-Producer 1" },
                                new List<Subscription>
                                {
                                    new Subscription
                                    {
                                        Id = 1,
                                        CoProducerId = "coproducer-1",
                                        ProductOfferId = 101,
                                        ExpectedQuantity = 5,
                                        PaymentType = PaymentType.Money
                                    },
                                    new Subscription
                                    {
                                        Id = 2,
                                        CoProducerId = "coproducer-1",
                                        ProductOfferId = 102,
                                        ExpectedQuantity = 3,
                                        PaymentType = PaymentType.BankTransfer
                                    }
                                },
                                new List<Payment>
                                {
                                    new Payment
                                    {
                                        Id = 1,
                                        SubscriptionId = 1,
                                        AmountPaid = 50.0,
                                        PaymentDate = DateTime.Now.AddDays(-5)
                                    },
                                    new Payment
                                    {
                                        Id = 2,
                                        SubscriptionId = 1,
                                        AmountPaid = 25.0,
                                        PaymentDate = DateTime.Now.AddDays(-2)
                                    },
                                    new Payment
                                    {
                                        Id = 3,
                                        SubscriptionId = 2,
                                        AmountPaid = 60.0,
                                        PaymentDate = DateTime.Now.AddDays(-3)
                                    }
                                }
                            )
                        },
                        {
                            "coproducer-2", new Tuple<AppUser, IEnumerable<Subscription>, IEnumerable<Payment>>(
                                new AppUser { Id = "coproducer-2", Name = "Co-Producer 2" },
                                new List<Subscription>
                                {
                                    new Subscription
                                    {
                                        Id = 3,
                                        CoProducerId = "coproducer-2",
                                        ProductOfferId = 103,
                                        ExpectedQuantity = 2,
                                        PaymentType = PaymentType.MBway
                                    }
                                },
                                new List<Payment>
                                {
                                    new Payment
                                    {
                                        Id = 4,
                                        SubscriptionId = 3,
                                        AmountPaid = 40.0,
                                        PaymentDate = DateTime.Now.AddDays(-1)
                                    }
                                }
                            )
                        }
                    };
                }
            });
        }
    }

    // Helper class to control the behavior of mocks
    public static class MockPaymentsInfoFlag
    {
        public static bool IsCoProducerPaymentsEmpty { get; set; } = false;
        public static bool IsProducerPaymentsEmpty { get; set; } = false;
        public static bool IsProducerToReceiveEmpty { get; set; } = false;
        public static bool IsCoProducerToPayEmpty { get; set; } = false;
        public static bool IsProducerTotalsEmpty { get; set; } = false;
        public static bool IsCoProducerTotalsEmpty { get; set; } = false;
        public static bool IsProducerCCEmpty { get; set; } = false;
        public static bool IsCoProducerCCEmpty { get; set; } = false;
    }
}
