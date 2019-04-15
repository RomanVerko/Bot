// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using WildWestLib;

namespace EchoBot2
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class EchoBot2Bot: IBot
    {
        
        private readonly EchoBot2Accessors _accessors;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="accessors">A class containing <see cref="IStatePropertyAccessor{T}"/> used to manage state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#windows-eventlog-provider"/>
        public EchoBot2Bot(EchoBot2Accessors accessors, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<EchoBot2Bot>();
            _logger.LogTrace("Turn start.");
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));
        }

        /// <summary>
        /// Every conversation turn for our Echo Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {


            bool firstPlay = true;
           
           

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Get the conversation state from the turn context.
                var state = await _accessors.CounterState.GetAsync(turnContext, () => new CounterState());

                // Bump the turn count for this conversation.
                state.TurnCount++;
                WWSource.MyCounter = state.TurnCount.ToString();
                WWSource.step++;
                

                // Set the property using the accessor.
                await _accessors.CounterState.SetAsync(turnContext, state);

                // Save the new turn count into the conversation state.
                await _accessors.ConversationState.SaveChangesAsync(turnContext);
               if (WWSource.step==100)
                {
                    var answer = turnContext.Activity.Text;
                    if (answer.Equals("К Шерифу за информацией")) {
                        WWSource.step = 7;
                    }
                    else if (answer.Equals("К подельнику за ружьем")){
                        WWSource.step = 11;
                    }
                    else if (answer.Equals("В салун, узнать про укрытие")){
                        WWSource.step = 15;
                    }
                    else {
                        await turnContext.SendActivityAsync("Чтож, пойдем к Шерифу.");
                        WWSource.step = 24;
                    }
                  


                }  
               switch (WWSource.step)
                {
                    case 1:
                        {
                            
                            WWSource.path1 = false;
                            WWSource.path2 = false;
                            WWSource.path3 = false;
                            await turnContext.SendActivityAsync(WWSource.GreetingSpeach);
                            var reply1 = turnContext.Activity.CreateReply();
                            var card = new AnimationCard {
                                //Title = ,
                                Media = new List<MediaUrl>()
                                    {
                                        new MediaUrl("https://popculturalstudies.files.wordpress.com/2016/06/gbu-3.gif")
                                    },
                                Subtitle = "Чуешь запах пороха, салага?"
                            };
                            //Add the attachment to our reply.
                            reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply1, cancellationToken);

                            //var reply = turnContext.Activity.CreateReply("What is your favorite color?");

                            //reply.SuggestedActions = new SuggestedActions() {
                            //    Actions = new List<CardAction>()
                            //    {
                            //       new CardAction() { Title = "Red", Type = ActionTypes.ImBack, Value = "Red" },
                            //       new CardAction() { Title = "Yellow", Type = ActionTypes.ImBack, Value = "Yellow" },
                            //       new CardAction() { Title = "Blue", Type = ActionTypes.ImBack, Value = "Blue" },
                            //    },

                            //};
                            //await turnContext.SendActivityAsync(reply, cancellationToken);
                            break;
                        }
                    case 2:
                        {
                            WWSource.Name = $"{WWSource.firstName[WWSource.rnd.Next(0, 10)]} {WWSource.secondName[WWSource.rnd.Next(0, 10)]}";
                            WWSource.GamerName = turnContext.Activity.Text;
                            await turnContext.SendActivityAsync($"{WWSource.GamerName}? Странное имя для Дикого Запада, не находишь? " +
                                $"С таким именем и помереть легче, чем оседлать мервого коня...\n\n" +
                                $"Я буду звать тебя {WWSource.Name}. Не пойми неправильно, тут свои законы.");

                            var reply = turnContext.Activity.CreateReply("Продолжить с этим именем?");

                            reply.SuggestedActions = new SuggestedActions() {
                                Actions = new List<CardAction>()
                                {
                                   new CardAction() { Title = "Да", Type = ActionTypes.ImBack, Value = "Да" },
                                   new CardAction() { Title = "Ругаться!", Type = ActionTypes.ImBack, Value =  $"Сам ты {WWSource.Name}! Меня зовут {WWSource.GamerName}, и никак по-другому!" },
                                },

                            };
                            await turnContext.SendActivityAsync(reply, cancellationToken);
                            break;
                        }

                    case 3:
                        { 
                            var temp = turnContext.Activity.Text;
                            if (temp.Equals("Да"))
                            {
                                var reply1 = turnContext.Activity.CreateReply();
                                var card = new AnimationCard {
                                    //Title = ,
                                    Media = new List<MediaUrl>()
                                        {
                                        new MediaUrl("https://i.pinimg.com/originals/8d/b7/df/8db7df62ffc99ac9333eb0fd4dcb3c89.gif")
                                    },
                                    
                                };
                                //Add the attachment to our reply.
                                reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                                // Send the activity to the user.
                                await turnContext.SendActivityAsync(reply1, cancellationToken);

                            }
                            else {
                                var reply = turnContext.Activity.CreateReply();

                                // Create an attachment.
                                var attachment = new Attachment {
                                    ContentUrl = "https://dz7u9q3vpd4eo.cloudfront.net/wp-content/legacy/posts/00846af2-277b-46aa-8382-282d4458411c.png",
                                    ContentType = "image/png",
                                    
                                };

                                // Add the attachment to our reply.
                                reply.Attachments = new List<Attachment>() { attachment };

                                // Send the activity to the user.
                                await turnContext.SendActivityAsync(reply, cancellationToken);
                                await turnContext.SendActivityAsync($"Перечить вздумал? Не нравится {WWSource.Name}?\nА ржавая пуля меж глаз понравится? " +
                                    $"Ты меня очень рассердил. Еще раз такое повторится, и я найду тебе имя похуже! Например, Назурбек.");
                                
                            }
                            await turnContext.SendActivityAsync(MakeButton("И это не обсуждается.", "Что дальше?", "Что дальше?", turnContext), cancellationToken);
                            break;
                        }
                    case 4:
                        {
                            await turnContext.SendActivityAsync(WWSource.Story1);
                            var reply1 = turnContext.Activity.CreateReply();
                            var card = new AnimationCard {
                                //Title = ,
                                Media = new List<MediaUrl>()
                                    {
                                        new MediaUrl("https://i.gifer.com/20ss.gif")
                                    },
                                Subtitle = "Амбар выглядел многообещающе"
                            };
                            //Add the attachment to our reply.
                            reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply1, cancellationToken);

                            await turnContext.SendActivityAsync(MakeButton("Мы решили его ограбить как только сядет солнце.", "Но что-то пошло не так?", "Но что-то пошло не так?", turnContext), cancellationToken);
                            break;
                        }
                    case 5:
                        {
                            await turnContext.SendActivityAsync(WWSource.Story2);
                            var reply1 = turnContext.Activity.CreateReply();
                            var card = new AnimationCard {
                                //Title = ,
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://u.kanobu.ru/comments/images/f6f29bb2-270a-40a6-8099-9b50763d7fd1.gif")
                                    },
                                Subtitle = ""
                            };
                            //Add the attachment to our reply.
                            reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply1, cancellationToken);

                            await turnContext.SendActivityAsync(MakeButton("Они еще поплатятся за это", "Согласиться", "Насколько я понимаю, выбора у меня нет?", turnContext), cancellationToken);
                            
                            break;
                        }
                    case 6:
                        {
                            await turnContext.SendActivityAsync("Вот именно.\nОднако, для успешного спасения Джея нам нехватает некоторых вещей, " +
                                "вот ими ты и займешься.");
                            await turnContext.SendActivityAsync(WWSource.Story3);
                            await turnContext.SendActivityAsync(WWSource.Story4);
                            await turnContext.SendActivityAsync(WWSource.Story5);
                            var reply1 = turnContext.Activity.CreateReply();
                            var card = new AnimationCard {
                                Title ="Вечером свидимся. Не подведи меня, странник" ,
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://i.ibb.co/51mZmGK/freegifmaker-me-2e3g-G.gif")
                                    },
                                Subtitle = "тыгыдык тыгыдык  *скачет по-дикозападски*"
                            };
                            //Add the attachment to our reply.
                            reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply1, cancellationToken);
                            

                            var activity = MessageFactory.Carousel(
                            new Attachment[]
                            {
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://i.ibb.co/WB1WkM7/467790021-16x9-1600.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "К Шерифу за информацией", type: ActionTypes.ImBack, value: "К Шерифу за информацией")
                                    })
                                .ToAttachment(),
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://i.ibb.co/GQFNdG2/1173467897-c8zrmnaaky.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "К подельнику за ружьем", type: ActionTypes.ImBack, value: "К подельнику за ружьем")
                                    })
                                .ToAttachment(),
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://i.ibb.co/3vzQH2z/big-photo.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "В салун, узнать про укрытие", type: ActionTypes.ImBack, value: "В салун, узнать про укрытие")
                                    })
                                .ToAttachment()
                            });

                            // Send the activity as a reply to the user.
                            WWSource.step = 99;
                            await turnContext.SendActivityAsync(activity);
                            
                            break;
                        }
                    case 7:
                        {
                            //var choice = turnContext.Activity.Text;
                            //if (choice.Equals("К Шерифу за информацией")) WWSource.path1 = true;
                            //if (choice.Equals("К подельнику за ружьем")) WWSource.path2 = true;
                            //if (choice.Equals("В салун, узнать про укрытие")) WWSource.path3 = true;

                            //todo other variants
                           // if (choice.Equals("К Шерифу за информацией"))
                            
                                var reply1 = turnContext.Activity.CreateReply();
                                var card = new AnimationCard {
                                    Title = "",
                                    Media = new List<MediaUrl>()
                                        {
                                       new MediaUrl("https://i.ibb.co/0jPpzb4/Webp-net-gifmaker.gif")
                                    },
                                    Subtitle = "*скачет по-дикозападски*"
                                };
                                //Add the attachment to our reply.
                                reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                                // Send the activity to the user.
                                await turnContext.SendActivityAsync(reply1, cancellationToken);

                                await turnContext.SendActivityAsync(MakeButton("А вот и шериф прогуливается по улице", "Подойти", $"Шериф, меня зовут {WWSource.Name}. " +
                                    $"Я прибыл из Оклахомы и слышал, что завтра у вас будет казнь. Можно ли на ней поприсутствовать?",turnContext));
                            
                            break;
                        }
                   case 8:
                        {
                            var reply1 = turnContext.Activity.CreateReply();
                            var card = new AnimationCard {
                                Title = "",
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://i.ibb.co/Q8wqfhK/freegifmaker-me-2e3o-U.gif")
                                    },
                                Subtitle = ""
                            };
                            //Add the attachment to our reply.
                            reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply1, cancellationToken);
                            await turnContext.SendActivityAsync($"Тот самый охотник, который убивает по 12 бизонов каждый день? Наслышан о вас, рад знакомству, {WWSource.Name}!" +
                                $" Для нас будет честь увидеть вас затра в 12. Мы наконец поймали одного из участников банды Хитрого Джо после драки в местном салуне. Его вешать и будем." +
                                $"\nКстати, вы слышали об этом Джо?");

                            var reply = turnContext.Activity.CreateReply();

                            reply.SuggestedActions = new SuggestedActions() {
                                Actions = new List<CardAction>()
                                {
                                   new CardAction() { Title = "Нет конечно", Type = ActionTypes.ImBack, Value = "Нет конечно, я в вашем городке всего пару часов\nК слову, где все это будет проходить?" },
                                   new CardAction() { Title = "Да, и я ему помогаю", Type = ActionTypes.ImBack, Value =  "Да, и я ему помогаю. Он послал меня узнать где именно будет казнь" },
                                },

                            };
                            await turnContext.SendActivityAsync(reply, cancellationToken);

                            break;
                        }
                    case 9:
                        {
                            var answer = "";
                            var choice = turnContext.Activity.Text;
                            if (choice.Equals("Нет конечно, я в вашем городке всего пару часов\nК слову, где все это будет проходить?")) answer = "Вот и хорошо, будьте осторожнее. Если вы у нас ненадолго, лучше вообще ни с кем не заводить " +
                                    "дружеских контактов - у нас любят обманывать таких туристов как вы.";
                            else answer = "Ахахахахах\nЧестно признаться, я вам почти поверил, пока вы это говорили! Все же будьте осторожнее. Если вы у нас ненадолго, лучше вообще ни с кем не заводить " +
                                    "дружеских контактов - у нас любят обманывать таких туристов как вы. ";
                            await turnContext.SendActivityAsync(MakeButton($"{answer}\nКазнь завтра на площади Линкольна - главной площади нашего города.", "Хорошо, спасибо", "Хорошо, спасибо, Шериф.\nПостораюсь быть.", turnContext));
                            WWSource.path1 = true;
                            break;
                        }
                    case 10:
                        {
                            IMessageActivity activity;

                            //todo menu
                            if (!(WWSource.path2 || WWSource.path3))
                            {
                                activity = MessageFactory.Carousel(
                            new Attachment[]
                            {
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://i.ibb.co/GQFNdG2/1173467897-c8zrmnaaky.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "К подельнику за ружьем", type: ActionTypes.ImBack, value: "К подельнику за ружьем")
                                    })
                                .ToAttachment(),
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://i.ibb.co/3vzQH2z/big-photo.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "В салун, узнать про укрытие", type: ActionTypes.ImBack, value: "В салун, узнать про укрытие")
                                    })
                                .ToAttachment()
                            });
                            } // ружье или дом
                            else if (WWSource.path2 == false && WWSource.path3 == true)
                            {
                                activity = MessageFactory.Carousel(
                            new Attachment[]
                            {
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://i.ibb.co/GQFNdG2/1173467897-c8zrmnaaky.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "К подельнику за ружьем", type: ActionTypes.ImBack, value: "К подельнику за ружьем")
                                    })
                                .ToAttachment()
                            });
                            } // ружье
                            else if (WWSource.path3 == false && WWSource.path2 == true)
                            {
                                activity = MessageFactory.Carousel(
                            new Attachment[]
                            {
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://i.ibb.co/3vzQH2z/big-photo.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "В салун, узнать про укрытие", type: ActionTypes.ImBack, value: "В салун, узнать про укрытие")
                                    })
                                .ToAttachment()
                            });
                            } // дом
                            else
                            {
                                activity = MessageFactory.Carousel(
                            new Attachment[]
                            {
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://st.overclockers.ru/images/soft/2012/09/06/gunslinger_01_big.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "На казнь", type: ActionTypes.ImBack, value: "Все сделано, пора спасать Джея")
                                    }) //todo отправить на казнь
                                .ToAttachment()
                            });
                            } // казнь
                            WWSource.step = 99;
                            // Send the activity as a reply to the user.
                            await turnContext.SendActivityAsync(activity);
                                break;
                        }
                    case 11:
                        {
                            //var choice = turnContext.Activity.Text;
                            //if (choice.Equals("К Шерифу за информацией")) WWSource.path1 = true;
                            //if (choice.Equals("К подельнику за ружьем")) WWSource.path2 = true;
                            //if (choice.Equals("В салун, узнать про укрытие")) WWSource.path3 = true;

                            ////todo other variants
                            //if (choice.Equals("К подельнику за ружьем"))
                            
                                var reply1 = turnContext.Activity.CreateReply();
                                var card = new AnimationCard {
                                    Title = "",
                                    Media = new List<MediaUrl>()
                                        {
                                       new MediaUrl("https://i.ibb.co/fQt70T0/Webp-net-gifmaker.gif")
                                    },
                                    Subtitle = "*скачет по-дикозападски*"
                                };
                                //Add the attachment to our reply.
                                reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                                // Send the activity to the user.
                                await turnContext.SendActivityAsync(reply1, cancellationToken);

                                await turnContext.SendActivityAsync(MakeButton("Этот человек похож на бандита.\nОн то нам и нужен!", "Поговорить", $"Я от Хитрого Джо, меня зовут {WWSource.Name}. Мне нужно отличное ружье для него", turnContext));
                            
                            break;
                        }
                    case 12:
                        {
                            var reply1 = turnContext.Activity.CreateReply();
                            var card = new AnimationCard {
                                Title = "",
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://i.ibb.co/BCVpHC3/freegifmaker-me-2e3pq.gif")
                                    },
                                Subtitle = ""
                            };
                            //Add the attachment to our reply.
                            reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply1, cancellationToken);

                            await turnContext.SendActivityAsync("Боже!\nНе подкрадывайся так! Меня ищут по всей округе, я прачусь от местного шерифа. Но раз ты меня нашел в этой глуши, " +
                                "значит ты действительно от нашего общего знакомого. Раз уж ты тут, говори для чего ему ружье! ");

                            var reply2 = turnContext.Activity.CreateReply();
                            var card1 = new AnimationCard {
                                Title = "",
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://i.ibb.co/QNmwj0p/freegifmaker-me-2e3qn.gif")
                                    },
                                Subtitle = "не нравится мне он"
                            };
                            //Add the attachment to our reply.
                            reply2.Attachments = new List<Attachment>() { card1.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply2, cancellationToken);

                            var reply = turnContext.Activity.CreateReply("Рассказать?");

                            reply.SuggestedActions = new SuggestedActions() {
                                Actions = new List<CardAction>()
                                {
                                   new CardAction() { Title = "Да", Type = ActionTypes.ImBack, Value = "Мы собираемся спасти Джея от повешания завтра на главной площади, надо будет стрелять с дальнего расстояния." },
                                   new CardAction() { Title = "Не совсем", Type = ActionTypes.ImBack, Value =  $"Кхм кхм..\n\nЭто дело не твое, а наше с бандой Хитрого Джо, ясно? " +
                                   $"Если не хочешь получить ржавую пулю меж глаз, ДАВАЙ РУЖЬЕ ИЛИ ЗАВАЛЮ ТЕБЯ КАК БУЙВОЛА НА ПОБЕРЕЖЬЕ МИСИССИПИ" },
                                },

                            };
                            await turnContext.SendActivityAsync(reply, cancellationToken);

                            break;
                        }
                    case 13:
                        {
                            var choice = turnContext.Activity.Text;
                            if (choice.Equals("Мы собираемся спасти Джея от повешания завтра на главной площади, надо будет стрелять с дальнего расстояния.")) {
                                await turnContext.SendActivityAsync("Джей всегда был слабым звеном, незачем его спасать, но если Джо думает иначе- его право.\n");
                            }
                            else { await turnContext.SendActivityAsync("ОКЕЙ ОКЕЙ СПОКОЙНЕЕ\nЧувствую, что ты уже понял банду Джо и дух нашего округа " ); }

                            await turnContext.SendActivityAsync(MakeButton("Ружье возьми у двери - это точно не подведет. Удачи ", "Уйти", "Джо свяжется с тобой. Спасибо", turnContext));
                            WWSource.path2 = true;
                            break;
                        }
                    case 14:
                        {
                            IMessageActivity activity;
                            //todo menu
                            if (!(WWSource.path1 || WWSource.path3))
                            {
                                activity = MessageFactory.Carousel(
                            new Attachment[]
                            {
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://i.ibb.co/WB1WkM7/467790021-16x9-1600.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "К Шерифу за информацией", type: ActionTypes.ImBack, value: "К Шерифу за информацией")
                                    })
                                .ToAttachment(),
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://i.ibb.co/3vzQH2z/big-photo.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "В салун, узнать про укрытие", type: ActionTypes.ImBack, value: "В салун, узнать про укрытие")
                                    })
                                .ToAttachment()
                            });
                            } // Шериф или Бармен
                            else if(WWSource.path1==true && WWSource.path3 == false)
                            {
                                activity = MessageFactory.Carousel(
                            new Attachment[]
                            {
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://i.ibb.co/3vzQH2z/big-photo.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "В салун, узнать про укрытие", type: ActionTypes.ImBack, value: "В салун, узнать про укрытие")
                                    })
                                .ToAttachment()
                            });
                            } // бармен
                            else if (WWSource.path1 == false && WWSource.path3 == true)
                            {
                                activity = MessageFactory.Carousel(
                            new Attachment[]
                            {
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://i.ibb.co/WB1WkM7/467790021-16x9-1600.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "К Шерифу за информацией", type: ActionTypes.ImBack, value: "К Шерифу за информацией")
                                    })
                                .ToAttachment()
                            });
                            } // шериф
                            else
                            {
                                activity = MessageFactory.Carousel(
                            new Attachment[]
                            {
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://st.overclockers.ru/images/soft/2012/09/06/gunslinger_01_big.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "На казнь", type: ActionTypes.ImBack, value: "Все сделано, пора спасать Джея")
                                    }) //todo отправить на казнь
                                .ToAttachment()
                            });
                            } // todo отправить на казнь
                            WWSource.step = 99;
                            await turnContext.SendActivityAsync(activity);

                            break;
                        }
                    case 15:
                        {
                            //var choice = turnContext.Activity.Text;
                            //if (choice.Equals("К Шерифу за информацией")) WWSource.path1 = true;
                            //if (choice.Equals("К подельнику за ружьем")) WWSource.path2 = true;
                            //if (choice.Equals("В салун, узнать про укрытие")) WWSource.path3 = true;

                            ////todo other variants
                            //if (choice.Equals("В салун, узнать про укрытие"))
                            
                                var reply1 = turnContext.Activity.CreateReply();
                                var card = new AnimationCard {
                                    Title = "",
                                    Media = new List<MediaUrl>()
                                        {
                                       new MediaUrl("https://i.ibb.co/whfwyQw/Webp-net-gifmaker.gif")
                                    },
                                    Subtitle = "*скачет по-дикозападски*"
                                };
                                //Add the attachment to our reply.
                                reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                                // Send the activity to the user.
                                await turnContext.SendActivityAsync(reply1, cancellationToken);

                                var reply2 = turnContext.Activity.CreateReply();
                                var card2 = new AnimationCard {
                                    
                                    Media = new List<MediaUrl>()
                                        {
                                       new MediaUrl("https://i.ibb.co/FqLMfpj/freegifmaker-me-2e3-FG.gif")
                                    }
                                    
                                };
                                //Add the attachment to our reply.
                                reply2.Attachments = new List<Attachment>() { card2.ToAttachment() };

                                // Send the activity to the user.
                                await turnContext.SendActivityAsync(reply2, cancellationToken);

                                await turnContext.SendActivityAsync(MakeButton("Салун - центр жизни окраины, а значит и центр похоти, алкоголя и грабежей. А человек, который находится здесь каждый" +
                                    " день - его бармен. Если уж мне нужен знающий все в этом городе, лучше него мне не найти.", "Поговорить", $"Я от Хитрого Джо, меня зовут {WWSource.Name}. Мне сказали, ты тут главный.", turnContext));

                            
                                break;
                        }
                    case 16:
                        {
                            var reply1 = turnContext.Activity.CreateReply();
                            var card = new AnimationCard {
                                Title = "",
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://i.ibb.co/3S1WgXR/freegifmaker-me-2e4z7.gif")
                                    },
                                Subtitle = "*Одну порцию дикозападского пойла, пожалуйста*"
                            };
                            //Add the attachment to our reply.
                            reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply1, cancellationToken);
                            await turnContext.SendActivityAsync("Я управляю этим заведением уже 13 долгих и тяжелых лет, что тебе нужно?\nДержу пари, это как-то связано " +
                                "с завтрашней казнью и другом Джо.");
                            await turnContext.SendActivityAsync(MakeButton("Ты ведь поэтому сейчас стоишь тут?\nНе очень то ты похож на любителя местной овощной водки",
                                "Ты прав", "Ты прав, мне нужно место для укрытия.\nМы собираемся спасти Джея от повешания. Для этого нам понадобится пустой дом, чтобы, сбежав" +
                                " от погони, залечь на дно. Есть что-то на примете?", turnContext));

                            break;
                        }
                    case 17:
                        {
                            await turnContext.SendActivityAsync($"Допустим, есть. \nОднако, не все так просто, {WWSource.Name}.");
                            await turnContext.SendActivityAsync($"Ты должен будешь сыграть со мной в игру, а если наберешь достаточное количество правильных ответов - " +
                                $"так и быть, дам вам на время свою старую конюшню под городом.");
                            var reply = turnContext.Activity.CreateReply("Готов?");

                            reply.SuggestedActions = new SuggestedActions() {
                                Actions = new List<CardAction>()
                                {
                                   new CardAction() { Title = "Ответить нормально", Type = ActionTypes.ImBack, Value = "Чтож, я принимаю твои условия! Поехали." },
                                   new CardAction() { Title = "Ответить по-дикозападски", Type = ActionTypes.ImBack, Value =  "Я человек без имени, который любит опасности, возможно я самый опасный человек во всем мире. Если кто захочет мне" +
                                   " перечить - пусть готовит гроб. А ты, бармен, если собрался играть со мной, готовься - с таким как я ты не играл никогда." },
                                    new CardAction() { Title = "Ответив, рассказав анекдот", Type = ActionTypes.ImBack, Value = "Кстати, анекдот!\nВ жаркий и пыльный день ковбой въезжает в небольшой городок. Соскочив с лошади, он подходит к ней сзади, поднимает и целует в то место, куда никогда не проникает солнце. " +
                                    "Старик, болтающий около расположенного рядом магазинчика, наблюдает за этой необычной сценой.\n\n- Зачем ты это сделал? - спрашивает он.\n- Губы потрескались, - отвечает ковбой.\n- И как, помогает?\n- Нет, но зато отбивает желание их постоянно облизывать." }
                                },
                            };
                            await turnContext.SendActivityAsync(reply, cancellationToken);

                            break;
                        }
                    case 18:
                        {
                            var answer = turnContext.Activity.Text;
                            if (answer.Equals("Чтож, я принимаю твои условия! Поехали.")) { await turnContext.SendActivityAsync("Отлично, тогда поехали"); }
                            else if (answer.Equals("Я человек без имени, который любит опасности, возможно я самый опасный человек во всем мире. Если кто захочет мне перечить - пусть готовит гроб. А ты, бармен, если собрался играть со мной, готовься - с таким как я ты не играл никогда.")) {
                                await turnContext.SendActivityAsync("Ууух, чую ковбойский дух! Какой ты молодец! Поехали играть!!");}
                            else { await turnContext.SendActivityAsync("AAAA ГоСпОдИ НО Я ЖЕ НЕ пРоСиЛ АнЕкДоТ!!!\nДавай уже играть в игру, и без таких историй, пожалуйста.\n\nЛучше бы как-нибудь по-ковбойски ответил.\n\nУжас."); }

                            var reply = turnContext.Activity.CreateReply("1) Классический вестерн родился в...");

                            reply.SuggestedActions = new SuggestedActions() {
                                Actions = new List<CardAction>()
                                {
                                   new CardAction() { Title = "Италии", Type = ActionTypes.ImBack, Value = "Италии" },
                                   new CardAction() { Title = "Германии", Type = ActionTypes.ImBack, Value =  "Германии" },
                                    new CardAction() { Title = "США", Type = ActionTypes.ImBack, Value = "США" }
                                },
                            };
                            await turnContext.SendActivityAsync(reply, cancellationToken);
                            break;
                        }
                    case 19:
                        {
                            var answer = turnContext.Activity.Text;
                            if (answer.Equals("США")) { await turnContext.SendActivityAsync("Конечно! Ты совершенно прав."); WWSource.GamePoints++; }
                            else { await turnContext.SendActivityAsync("К сожалению, нет."); }

                            await turnContext.SendActivityAsync("Безусловно, в Америке, и посвящен этот жанр, само название которого означает «западный», покорению Дикого Запада. Благородные (и не очень) ковбои, кровожадные (чаще всего) индейцы, певицы (и просто девушки легкого поведения) — обитательницы салунов и борделей, шерифы, бандиты, честные протестантские труженики, осваивающие новые земли, — вот обитатели классического вестерна, история которого ведется с 1903 года, когда Эдвин Стэнтон Портер снял «Большое ограбление поезда». ");

                            var reply = turnContext.Activity.CreateReply();

                            // Create an attachment.
                            var attachment = new Attachment {
                                ContentUrl = "http://s7.travelask.ru/system/images/files/000/329/929/full/the-wild-westfotor.jpg?1501132124",
                                ContentType = "image/png",

                            };

                            // Add the attachment to our reply.
                            reply.Attachments = new List<Attachment>() { attachment };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply, cancellationToken);

                            var reply1 = turnContext.Activity.CreateReply("2) География вестерна — пустынные ландшафты, которые только предстоит покорить. Что возводится в новых городах в последнюю очередь?");

                            reply1.SuggestedActions = new SuggestedActions() {
                                Actions = new List<CardAction>()
                                {
                                   new CardAction() { Title = "Церковь", Type = ActionTypes.ImBack, Value = "Церковь" },
                                   new CardAction() { Title = "Школа", Type = ActionTypes.ImBack, Value =  "Школа" },
                                   new CardAction() { Title = "Бордель", Type = ActionTypes.ImBack, Value = "Бордель" }
                                },
                            };
                            await turnContext.SendActivityAsync(reply1, cancellationToken);
                            break;
                        }
                    case 20:
                        {
                            var answer = turnContext.Activity.Text;
                            if (answer.Equals("Школа")) { await turnContext.SendActivityAsync("Конечно! Ты совершенно прав."); WWSource.GamePoints++; }
                            else { await turnContext.SendActivityAsync("К сожалению, нет."); }

                            await turnContext.SendActivityAsync("Уж точно не бордель — в фильме времен золотой лихорадки «Доусон Сити: Замороженное время» упоминается интересный факт: с публичных домов началось состояние семейства Трампов. Но бордели становятся и первыми жертвами цивилизации: стоит установиться закону и порядку, злачные места идут под снос. Слава Богу, вестерны не интересуются этими скучными временами.");

                            var reply = turnContext.Activity.CreateReply();

                            // Create an attachment.
                            var attachment = new Attachment {
                                ContentUrl = "http://restinworld.ru/nuke/objects/countries_stories/4/4wf10h869kxvqbtlx3w2vdm6tkt047d3/image/LeeDubin-WesternSaloon.jpg",
                                ContentType = "image/png",

                            };

                            // Add the attachment to our reply.
                            reply.Attachments = new List<Attachment>() { attachment };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply, cancellationToken);

                            var reply1 = turnContext.Activity.CreateReply("3) Без какого оружия немыслим вестерн?");

                            reply1.SuggestedActions = new SuggestedActions() {
                                Actions = new List<CardAction>()
                                {
                                   new CardAction() { Title = "Ружье Винчестера", Type = ActionTypes.ImBack, Value = "Ружье Винчестера" },
                                   new CardAction() { Title = "Револьвер Кольта", Type = ActionTypes.ImBack, Value =  "Револьвер Кольта" },
                                   new CardAction() { Title = "Пулемет Гатлинга", Type = ActionTypes.ImBack, Value = "Пулемет Гатлинга" }
                                },
                            };
                            await turnContext.SendActivityAsync(reply1, cancellationToken);

                            break;
                        }
                    case 21:
                        {
                            var answer = turnContext.Activity.Text;
                            if (answer.Equals("Пулемет Гатлинга")) { await turnContext.SendActivityAsync("Ты не прав!");  }
                            else { await turnContext.SendActivityAsync("О да, это оружие - одно из главных памятников Америки времен Дикого Запада"); WWSource.GamePoints++;}

                            var activity = MessageFactory.Carousel(
                            new Attachment[]
                            {
                                new HeroCard(
                                    title: "Ружье Винчестера 1872",
                                    images: new CardImage[] { new CardImage(url: "http://sportingshot.ru/assets/uploads/2016_07_28_10_17_3a107ec153aa1576462b8.jpg") }
                                    )
                                .ToAttachment(),
                                new HeroCard(
                                    title: "Револьвер Кольта 1873",
                                    images: new CardImage[] { new CardImage(url: "http://sportingshot.ru/assets/uploads/05-colt-sa-revolver.jpg") })
                                .ToAttachment()
                            });

                            // Send the activity as a reply to the user.
                            await turnContext.SendActivityAsync(activity);
                            await turnContext.SendActivityAsync("Ведущий начало от винтовки Генри времён Гражданской войны, карабин  Winchester Model 1886, часто именуемый «Yellow Boy» за свою бронзовую ствольную коробку, был популярен как у индейцев, так и солдат и поселенцев, направлявшихся на Запад. В вариантах винтовки, мушкета и карабина, эти Винчестеры рычажного действия могли иметь более дюжины патронов в своих трубчатых магазинах, а такая огневая мощь имела большое значение для выживания.");
                            await turnContext.SendActivityAsync("Револьвер Кольта. \nМногозарядное огнестрельное оружие, сошедшее с конвейера Сэмюэля Кольта, — продолжение ковбойской руки. Гатлингами пионеры фронтира тоже пользовались, правда, реже и далеко не во всех фильмах.");

                            var reply = turnContext.Activity.CreateReply("Последний, пятый вопрос:\n\nВестерны итальянца Серджо Леоне — «За пригоршню долларов», «На несколько долларов больше», «Хороший, Плохой, Злой» — сделали звездой Клинта Иствуда и породили великий поджанр в фильмографии. С тех пор национальным разновидностям жанра не раз присваивали гастрономические определения. " +
                                "Как назывался поджанр?");

                            reply.SuggestedActions = new SuggestedActions() {
                                Actions = new List<CardAction>()
                                {
                                   new CardAction() { Title = "Спагетти-вестерн", Type = ActionTypes.ImBack, Value = "Спагетти-вестерн" },
                                   new CardAction() { Title = "Гаспаччо-вестерн", Type = ActionTypes.ImBack, Value =  "Гаспаччо-вестерн" },
                                   new CardAction() { Title = "Борщ-вестерн", Type = ActionTypes.ImBack, Value = "Борщ-вестерн" }
                                },
                            };
                            await turnContext.SendActivityAsync(reply, cancellationToken);
                            break;
                        }
                    case 22:
                        {
                            var answer = turnContext.Activity.Text;
                            if (answer.Equals("Спагетти-вестерн")) { await turnContext.SendActivityAsync("В точку!"); WWSource.GamePoints++;}
                            else { await turnContext.SendActivityAsync("Нет, в Италии едят спагетти! Именно поэтому жанр назвали спагетти-вестерн.");  }
                            await turnContext.SendActivityAsync("«Борщ-вестернами», понятное дело, называли опыты Восточного блока. Так зарубежные злопыхатели окрестили «Белое солнце пустыни». В книге британского историка Кристофера Фрейлинга «Спагетти-вестерны. Ковбои и европейцы от Карла Мая до Серджо Леоне» «борщ-вестерн» иллюстрируется фильмом Владимира Вайнштока «Вооружен и очень опасен» и упоминается наряду с «паэлья-вестерном» (Испания), «карри-вестерном» (Индия), «камамбер-вестерном» (Франция) и «квашеная-капуста-вестерном» (Германия); оригинальные фильмы с Джоном Уэйном Фрейлинг предлагает называть «гамбургер-вестернами». Корейский режиссер Ким Чжи Ун сам определил свой фильм 2008 года «Хороший, плохой, долбанутый» «кимчи-вестерном». ");
                            
                            await turnContext.SendActivityAsync(MakeButton($"Ну все, игра окончена. Сумма твоих очков - {WWSource.GamePoints} \nТак и быть, забирайте дом на ранчо Большого Джо, завтра там будет свободно.","Поблагодарить и уйти", "Отличная игра, спасибо за Дом.", turnContext));
                            WWSource.path3 = true;
                            break;
                        }
                    case 23:
                        {
                            IMessageActivity activity;
                            if (!(WWSource.path1 || WWSource.path2)) {
                                activity = MessageFactory.Carousel(
                            new Attachment[]
                            {
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://i.ibb.co/WB1WkM7/467790021-16x9-1600.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "К Шерифу за информацией", type: ActionTypes.ImBack, value: "К Шерифу за информацией")
                                    })
                                .ToAttachment(),
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://i.ibb.co/GQFNdG2/1173467897-c8zrmnaaky.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "К подельнику за ружьем", type: ActionTypes.ImBack, value: "К подельнику за ружьем")
                                    })
                                .ToAttachment()
                            });
                            } // Шериф или поддельник
                            else if (WWSource.path1 == true && WWSource.path2 == false) {
                                activity = MessageFactory.Carousel(
                            new Attachment[]
                            {
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://i.ibb.co/GQFNdG2/1173467897-c8zrmnaaky.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "К подельнику за ружьем", type: ActionTypes.ImBack, value: "К подельнику за ружьем")
                                    })
                                .ToAttachment()
                            });

                            } // к поддельнику
                            else if (WWSource.path1 == false && WWSource.path2 == true) {

                                activity = MessageFactory.Carousel(
                            new Attachment[]
                            {
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://i.ibb.co/WB1WkM7/467790021-16x9-1600.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "К Шерифу за информацией", type: ActionTypes.ImBack, value: "К Шерифу за информацией")
                                    })
                                .ToAttachment()
                            });
                            } // к шерифу
                            else {
                                activity = MessageFactory.Carousel(
                            new Attachment[]
                            {
                                new HeroCard(
                                    title: "",
                                    images: new CardImage[] { new CardImage(url: "https://st.overclockers.ru/images/soft/2012/09/06/gunslinger_01_big.jpg") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "На казнь", type: ActionTypes.ImBack, value: "Все сделано, пора спасать Джея")
                                    }) //todo отправить на казнь
                                .ToAttachment()
                            });
                            } //Казнь
                            
                            WWSource.step = 99;
                            await turnContext.SendActivityAsync(activity);
                            break;
                        }
                    case 24:
                        {
                            await turnContext.SendActivityAsync("Отправлюсь на площадь, именно там Хитрый Джо договорился встретиться после всех дел.");
                            var reply1 = turnContext.Activity.CreateReply();
                            var card = new AnimationCard {
                                Title = "Славный у нас городок, не так ли?",
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://i.ibb.co/kc8z03N/freegifmaker-me-2e7-Go.gif")
                                    },
                                Subtitle = "*в воздухе витал дух смерти*"
                            };
                            //Add the attachment to our reply.
                            reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply1, cancellationToken);
                            await turnContext.SendActivityAsync(MakeButton($"{WWSource.Name}, ты сделал, то что я просил?", "Да, все","Да, я сделал все. Взял ружье у твоего друга, договорился насчет дома возле ранчо, а так же узнал место казни.", turnContext));
                            break;
                        }
                    case 25:
                        {
                            var reply1 = turnContext.Activity.CreateReply();
                            var card = new AnimationCard {
                                Title = "Хорошо, как только все закончится я помогу тебе уехать",
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://i.ibb.co/pfrqxfW/freegifmaker-me-2e7-H3.gif")
                                    },
                                Subtitle = ""
                            };
                            //Add the attachment to our reply.
                            reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply1, cancellationToken);

                            await turnContext.SendActivityAsync(MakeButton($"И где же будет происходить повешание?", "На площади Линкольна", "На площади Линкольна, как сказал мне Шериф. Он хороший человек, даже пригласил меня в первые ряды завтра, " +
                                "будет обидно, если он узнает, зачем я это у него спрашивал.", turnContext));
                            break;
                        }
                    case 26:
                        {
                            
                            

                            var reply1 = turnContext.Activity.CreateReply();
                            var card = new AnimationCard {
                                Title = "",
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://i.ibb.co/WxSThR8/freegifmaker-me-2e7-Hr.gif")
                                    },
                                Subtitle = ""
                            };
                            //Add the attachment to our reply.
                            reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply1, cancellationToken);
                            await turnContext.SendActivityAsync(MakeButton("Да, ты прав, очень уж ты похож на мирного гражданина.\nНу хорошо, спасибо тебе. Увидимся завтра. Все должно получиться.", "Уйти", "До завтра, друг мой.", turnContext));

                            break;
                            
                        }
                    case 27:
                        {
                            var reply1 = turnContext.Activity.CreateReply();
                            var card = new AnimationCard {
                                Title = "",
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://i.ibb.co/mSwLdL2/freegifmaker-me-2e7-HJ.gif")
                                    },
                                Subtitle = ""
                            };
                            //Add the attachment to our reply.
                            reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply1, cancellationToken);
                            await turnContext.SendActivityAsync("Сложный сегодня предстоит день.");

                            var reply2 = turnContext.Activity.CreateReply();
                            var card2 = new AnimationCard {
                                Title = "",
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://media.giphy.com/media/sRFu7D9SKk6JB1R1x0/giphy.gif")
                                    },
                                Subtitle = ""
                            };
                            //Add the attachment to our reply.
                            reply2.Attachments = new List<Attachment>() { card2.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply2, cancellationToken);

                            await turnContext.SendActivityAsync("А вот и шериф везет Джея.. Да, несладко ему придется, если мы не поможем");

                            var reply3 = turnContext.Activity.CreateReply();
                            var card3 = new AnimationCard {
                                Title = "",
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://media.giphy.com/media/37qbJPMZMOUKejnmCl/giphy.gif")
                                    },
                                Subtitle = ""
                            };
                            //Add the attachment to our reply.
                            reply3.Attachments = new List<Attachment>() { card3.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply3, cancellationToken);

                            var reply4 = turnContext.Activity.CreateReply();
                            var card4 = new AnimationCard {
                                Title = "Пора действовать",
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://media.giphy.com/media/5bmEURB16TdCow6oug/giphy.gif")
                                    },
                                Subtitle = "*ох что сейчас будет...*"
                            };
                            //Add the attachment to our reply.
                            reply4.Attachments = new List<Attachment>() { card4.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply4, cancellationToken);
                            await turnContext.SendActivityAsync(MakeButton("Самое время", "Спасти Джея", "Стреляй!", turnContext));
                            break;
                        }
                    case 28:
                        {
                            var reply1 = turnContext.Activity.CreateReply();
                            var card = new AnimationCard {
                                Title = "",
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://media.giphy.com/media/14O9PWwg81VXTZvH5Y/giphy.gif")
                                    },
                                Subtitle = ""
                            };
                            //Add the attachment to our reply.
                            reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply1, cancellationToken);
                            await turnContext.SendActivityAsync(MakeButton("Дело сделано", "Бежать", "Уходим отсюда!", turnContext));
                            break;
                        }
                    case 29:
                        {
                            var reply1 = turnContext.Activity.CreateReply();
                            var card = new AnimationCard {
                                Title = "",
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://media.giphy.com/media/elNqAyY4vGizLEsJXw/giphy.gif")
                                    },
                                Subtitle = ""
                            };
                            //Add the attachment to our reply.
                            reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply1, cancellationToken);

                            await turnContext.SendActivityAsync(MakeButton("Надо встретиться с Джеем на ранчо", "Ехать к Джею", "А вот и Джей!", turnContext));
                            break;

                        }
                    case 30:
                        {
                            var reply1 = turnContext.Activity.CreateReply();
                            var card = new AnimationCard {
                                Title = "Джо, как.. Как ты это сделал?",
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://media.giphy.com/media/4KFvmzvehaydYCWM0T/giphy.gif")
                                    },
                                Subtitle = "*Наконец-то, чувство свободы*"
                            };

                            //Add the attachment to our reply.
                            reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply1, cancellationToken);
                            await turnContext.SendActivityAsync($"Скажем так, это не только благодаря мне. Все подготовил {WWSource.Name}. Все благодаря нему" +
                                $" Поживешь в этом доме пару дней, пока не оправишься, а там дальше за работу. Золото не ждет, да и банда Хэтфилда пока сидит тихо - это наш шанс.");
                            await turnContext.SendActivityAsync(MakeButton($"Вот и пришло время прощаться, {WWSource.Name}. Мне было приятно с тобой работать. Если что, заезжай, мы всегда будем тебе рады.", "Попрощаться", "Благодаря тебе " +
                                "я понял что такое дикий запад и попробовал его на вкус. Я еще вернусь, Джо, не сомневайся.", turnContext));
                            break;
                        }
                    case 31:
                        {
                            var reply1 = turnContext.Activity.CreateReply();
                            var card = new AnimationCard {
                                Title = "До скорой встречи.",
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://media.giphy.com/media/mXiOYnCLaOp1mCVtcc/giphy.gif")
                                    },
                                Subtitle = "*грустное тыгыдык-тыгыдыканье...*"
                            };

                            //Add the attachment to our reply.
                            reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply1, cancellationToken);
                            await turnContext.SendActivityAsync(MakeButton("Конец", "Конец?", "Да, конец.", turnContext));
                            break;

                        }
                    case 32:
                        {
                            var reply1 = turnContext.Activity.CreateReply();
                            var card = new AnimationCard {
                                Title = "",
                                Media = new List<MediaUrl>()
                                    {
                                       new MediaUrl("https://media2.giphy.com/media/oRY6fDRIpCz3q/giphy.gif?cid=3640f6095c44fd8f48584c756fec95ff.gif")
                                    },
                                Subtitle = "Roman Verko, 2019"
                            };

                            //Add the attachment to our reply.
                            reply1.Attachments = new List<Attachment>() { card.ToAttachment() };

                            // Send the activity to the user.
                            await turnContext.SendActivityAsync(reply1, cancellationToken);
                            await turnContext.SendActivityAsync(MakeButton("Начать заново?", "Начать заново", "Начать заново", turnContext));
                            WWSource.step = 0;
                            break;
                        }
                        
                    //default:
                    //    {
                    //        var choice = turnContext.Activity.Text;
                    //        await turnContext.SendActivityAsync(WWSource.MyCounter + choice);
                    //        break;
                    //    }

                }
                
            }
            else if (firstPlay)
            {
                await turnContext.SendActivityAsync(MakeButton("Начнём незабываемое путешествие?", "Да!!","В путь",turnContext), cancellationToken);
                firstPlay = false;
                WWSource.step = 0;
                WWSource.GamePoints = 0;
            }
        }
        /// <summary>
        /// Создание кнопки для продолжения диалога
        /// </summary>
        /// <param name="Question">Вопрос</param>
        /// <param name="NewTitle">Надпись на кнопке</param>
        /// <param name="NewValue">Посылаемое сообщение</param>
        /// <param name="NewTurnContext">пишем turnContext</param>
        /// <returns></returns>
        public static Activity MakeButton(string Question,string NewTitle, string NewValue, ITurnContext NewTurnContext)
        {
            Activity StartButton = NewTurnContext.Activity.CreateReply(Question);
            StartButton.SuggestedActions = new SuggestedActions() {
                Actions = new List<CardAction>()
                    {
                        new CardAction() { Title = NewTitle, Type = ActionTypes.ImBack, Value = NewValue },
                    },

            };
            return StartButton;

        }
    }
}
