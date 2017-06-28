using System;
namespace Stepquencer
{
    public class Author
    {
        private String name;
        private bool hasMultipleIcons;
        private String personalLink;
        private string affiliateLink;

        public String Name
        {
            get { return name;}
            set { name = value;}
        }

        public bool HasMultipleIcons
        {
            get { return hasMultipleIcons;}
            set { hasMultipleIcons = value;}
        }

        public String PersonalLink
        {
            get { return personalLink;}
            set { personalLink = value;}
        }

        public String AffiliateLink
        {
            get { return affiliateLink;}
            set { affiliateLink = value;}
        }

        public Author(String name, String hasMultipleIcons, String personalLink, String affiliateLink)
        {
            this.name = name;
            this.hasMultipleIcons = hasMultipleIcons.Equals("T");
            this.personalLink = personalLink;
            this.affiliateLink = affiliateLink;
        }

        public Author(String name, String hasMultipleIcons, String personalLink)
        {
	        this.name = name;
	        this.hasMultipleIcons = hasMultipleIcons.Equals("T");
	        this.personalLink = personalLink;
	        this.affiliateLink = "";
        }
    }
}
