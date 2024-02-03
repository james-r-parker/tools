import Link from "next/link";

type CmsResponse = {
    data: {
        pages: {
            createdAt: string,
            slug: string,
            title: string,
        }[]
    }
};

type CmsMenuItem = {
    text: string,
    link: string,
}

async function getMenuItems(): Promise<CmsMenuItem[]> {
    try {
        const response = await fetch(process.env.NEXT_HYGRAPH_API_URL!, {
            next: {
                revalidate: 900,
            },
            method: 'POST',
            headers: {
                'content-type': 'application/json'
            },
            body: JSON.stringify({
                query: `query Pages {
                    pages {
                      createdAt,
                      slug,
                      title,
                    }
                  }`,
            }),
        });
        const result = await response.json() as CmsResponse;
        console.debug('RESPONSE FROM FETCH REQUEST', result.data.pages);

        return result.data.pages.map(page => {
            return {
                text: page.title,
                link: `/${page.slug}`
            };
        });
    }
    catch (err) {
        console.error('ERROR DURING FETCH REQUEST', err);
    }

    return [];
}

function MenuItem({ text, link }: { text: string, link: string }) {
    return (
        <span className="block border-r pr-4 -mr-px border-transparent border-slate-400 dark:border-slate-500 text-slate-700 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-300">
            <Link className="text-sm" href={link}>
                {text}
            </Link>
        </span>
    );
}

export default async function MainMenu() {

    const menu = await getMenuItems();

    return (
        <nav>
            <div className="flex gap-3">
                {menu.map(item => {
                    return <MenuItem key={item.link} text={item.text} link={item.link} />
                })}
                <span>
                    <Link className="text-sm" href={"https://www.dotnethelp.co.uk"}>
                        Developer Tools
                    </Link>
                </span>
            </div>
        </nav>
    );
}
