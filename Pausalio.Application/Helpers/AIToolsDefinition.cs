namespace Pausalio.Application.Helpers
{
    namespace Pausalio.Application.Helpers
    {
        public static class AIToolsDefinition
        {
            public static object[] GetTools() => new object[]
            {
                // ===== KLIJENTI =====
                new {
                    type = "function",
                    function = new {
                        name = "get_top_clients",
                        description = "Vraća listu klijenata sortiranih po ukupno fakturisanom iznosu. Koristi kada korisnik pita o najboljim klijentima, najvećim klijentima, najboljoj saradnji ili stranim/domaćim klijentima.",
                        parameters = new {
                            type = "object",
                            properties = new {
                                top = new {
                                    type = "integer",
                                    description = "Koliko klijenata da vrati. Ako korisnik ne kaže broj, koristi 5."
                                },
                                clientType = new {
                                    type = "string",
                                    description = "Opcioni filter po tipu klijenta. Moguće vrednosti: Individual, Domestic, Foreign. Ako korisnik ne navede tip, ne šalji ovaj parametar."
                                }
                            },
                            required = new[] { "top" }
                        }
                    }
                },

                // ===== FAKTURE =====
                new {
                    type = "function",
                    function = new {
                        name = "get_invoices_by_status",
                        description = "Vraća fakture filtrirane po statusu fakture.",
                        parameters = new {
                            type = "object",
                            properties = new {
                                status = new {
                                    type = "string",
                                    description = "Moguće vrednosti: Draft, Sent, Finished, Cancelled, Archived"
                                }
                            },
                            required = new[] { "status" }
                        }
                    }
                },
                new {
                    type = "function",
                    function = new {
                        name = "get_invoices_by_payment_status",
                        description = "Vraća fakture filtrirane po statusu plaćanja. Koristi kada korisnik pita o neplaćenim, plaćenim fakturama ili naplati.",
                        parameters = new {
                            type = "object",
                            properties = new {
                                paymentStatus = new {
                                    type = "string",
                                    description = "Moguće vrednosti: Unpaid, Paid, PartiallyPaid"
                                }
                            },
                            required = new[] { "paymentStatus" }
                        }
                    }
                },
                new {
                    type = "function",
                    function = new {
                        name = "get_invoices_by_year",
                        description = "Vraća sve fakture za određenu godinu. Koristi kada korisnik pita o fakturama u određenoj godini ili periodu.",
                        parameters = new {
                            type = "object",
                            properties = new {
                                year = new {
                                    type = "integer",
                                    description = "Godina za koju se traže fakture. Ako nije navedena koristi tekuću godinu."
                                }
                            },
                            required = new[] { "year" }
                        }
                    }
                },
                new {
                    type = "function",
                    function = new {
                        name = "get_overdue_invoices",
                        description = "Vraća sve fakture kojima je prošao rok plaćanja a još nisu plaćene. Koristi kada korisnik pita o zakasnelim fakturama, dugovima klijenata ili nenaplaćenim potraživanjima.",
                        parameters = new {
                            type = "object",
                            properties = new { },
                            required = new string[] { }
                        }
                    }
                },
                new {
                    type = "function",
                    function = new {
                        name = "get_invoice_summary",
                        description = "Vraća sumarni pregled svih faktura — ukupno fakturisano, naplaćeno, nenaplaćeno, broj faktura po statusu. Koristi kada korisnik traži pregled ili statistiku faktura.",
                        parameters = new {
                            type = "object",
                            properties = new { },
                            required = new string[] { }
                        }
                    }
                },

                // ===== TROŠKOVI =====
                new {
                    type = "function",
                    function = new {
                        name = "get_expenses_by_status",
                        description = "Vraća troškove filtrirane po statusu.",
                        parameters = new {
                            type = "object",
                            properties = new {
                                status = new {
                                    type = "string",
                                    description = "Moguće vrednosti: Pending, Paid, Archived"
                                }
                            },
                            required = new[] { "status" }
                        }
                    }
                },
                new {
                    type = "function",
                    function = new {
                        name = "get_expense_summary",
                        description = "Vraća sumarni pregled troškova — ukupno, plaćeno, na čekanju, arhivirano. Koristi kada korisnik pita o ukupnim troškovima ili finansijskom stanju troškova.",
                        parameters = new {
                            type = "object",
                            properties = new { },
                            required = new string[] { }
                        }
                    }
                },

                // ===== POREZI =====
                new {
                    type = "function",
                    function = new {
                        name = "get_tax_obligations_by_year",
                        description = "Vraća poreske obaveze za određenu godinu.",
                        parameters = new {
                            type = "object",
                            properties = new {
                                year = new {
                                    type = "integer",
                                    description = "Godina za koju se traže poreske obaveze. Ako nije navedena koristi tekuću godinu."
                                }
                            },
                            required = new[] { "year" }
                        }
                    }
                },
                new {
                    type = "function",
                    function = new {
                        name = "get_tax_obligations_by_status",
                        description = "Vraća poreske obaveze filtrirane po statusu. Koristi kada korisnik pita o neplaćenim ili plaćenim porezima.",
                        parameters = new {
                            type = "object",
                            properties = new {
                                status = new {
                                    type = "string",
                                    description = "Moguće vrednosti: Pending, Paid, Archived"
                                }
                            },
                            required = new[] { "status" }
                        }
                    }
                },
                new {
                    type = "function",
                    function = new {
                        name = "get_overdue_taxes",
                        description = "Vraća poreske obaveze kojima je prošao rok a još nisu plaćene. Koristi kada korisnik pita o zakasnelim porezima ili poreskim dugovima.",
                        parameters = new {
                            type = "object",
                            properties = new { },
                            required = new string[] { }
                        }
                    }
                },
                new {
                    type = "function",
                    function = new {
                        name = "get_tax_summary",
                        description = "Vraća sumarni pregled poreskih obaveza — ukupno neplaćeno, plaćeno, zakasnelo. Koristi kada korisnik pita o poreskom stanju ili dugovima.",
                        parameters = new {
                            type = "object",
                            properties = new {
                                year = new {
                                    type = "integer",
                                    description = "Opciona godina. Ako nije navedena vraća sve."
                                }
                            },
                            required = new string[] { }
                        }
                    }
                },

                // ===== PRIHODI =====
                new {
                    type = "function",
                    function = new {
                        name = "get_monthly_income",
                        description = "Vraća ukupan fakturisani prihod po mesecima za određenu godinu. Koristi kada korisnik pita o mesečnim prihodima, trendu prihoda ili poređenju meseci.",
                        parameters = new {
                            type = "object",
                            properties = new {
                                year = new {
                                    type = "integer",
                                    description = "Godina za koju se traži mesečni prihod. Ako nije navedena koristi tekuću godinu."
                                }
                            },
                            required = new[] { "year" }
                        }
                    }
                },
                new {
                    type = "function",
                    function = new {
                        name = "get_income_vs_expenses",
                        description = "Vraća poređenje ukupnih prihoda i troškova za određenu godinu. Koristi kada korisnik pita o profitabilnosti, neto prihodu ili odnosu prihoda i rashoda.",
                        parameters = new {
                            type = "object",
                            properties = new {
                                year = new {
                                    type = "integer",
                                    description = "Godina za poređenje. Ako nije navedena koristi tekuću godinu."
                                }
                            },
                            required = new[] { "year" }
                        }
                    }
                }
            };
        }
    }
}