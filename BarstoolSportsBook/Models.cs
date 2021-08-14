namespace BarstoolSportsBook
{
    public class Matches
    {
        public Event[] Events;
    }

    public class Event
    {
        public BetOffer[] BetOffers;
    }

    public class BetOffer
    {
        public BetOfferType BetOfferType;
        public Outcome[] Outcomes;
    }

    public class Outcome
    {
        public int odds;
        public int? line;
        public string participant;
    }

    public class BetOfferType
    {
        public string Name;
    }
}
