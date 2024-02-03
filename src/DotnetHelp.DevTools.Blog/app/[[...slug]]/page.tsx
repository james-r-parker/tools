import { Metadata, ResolvingMetadata } from "next";
import CmsContentRender from "@/components/rich-text";

type Page = {
    createdAt: string,
    slug: string,
    title: string,
    overview: string,
    body: {
        json: any
    }
}

type CmsResponse = {
    data: {
        page: Page
    }
};

type CmsPagesResponse = {
    data: {
        pages: {
            createdAt: string,
            slug: string,
            title: string,
        }[]
    }
};

type CmsMetaResponse = {
    data: {
        page: {
            metadata: {
                title: string,
                description: string,
                image: {
                    width: number,
                    height: number,
                    url: string
                }
            }
        }
    }
};

async function getPage(slug: string): Promise<Page | null> {
    try {
        console.debug('FETCHING PAGE', slug);
        const response = await fetch(process.env.NEXT_HYGRAPH_API_URL!, {
            next: {
                revalidate: 900,
            },
            method: 'POST',
            headers: {
                'content-type': 'application/json'
            },
            body: JSON.stringify({
                query: `query getPage($slug:String!) {
                    page(where:{slug:$slug}) {
                      createdAt,
                      slug,
                      title,
                      overview,
                      body {
                        json
                      }
                    }
                  }`,
                variables: { slug }
            }),
        });
        const result = await response.json() as CmsResponse;
        console.debug('PAGE', result);
        return result.data.page;
    }
    catch (err) {
        console.error('ERROR DURING FETCH REQUEST', err);
    }

    return null;
}

export default async function Page({ params }: { params: { slug: string[] } }) {
    const page = await getPage(params.slug?.join('/') || 'home');

    if (!page) {
        return <div>404</div>;
    }

    return <article>
        <header className="mb-5 md:flex md:items-start">
            <div className="flex-auto max-w-4xl">
                <h1 className="mb-4 text-sm leading-6 font-semibold text-sky-500 dark:text-sky-400">{page.title}</h1>
                <h2 className="text-3xl sm:text-4xl font-extrabold text-slate-900 tracking-tight dark:text-slate-200">{page.overview}</h2>
            </div>
        </header>
        <CmsContentRender options={{ content: page.body.json }} />
    </article>
}

export async function generateStaticParams() {
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
                  slug
                }
              }`,
        }),
    });
    const result = await response.json() as CmsPagesResponse;
    console.debug('ALL PAGES', result.data.pages);

    return result.data.pages.map((post) => ({
        slug: [post.slug],
    })).concat({ slug: [''] });
}

export async function generateMetadata({ params }: { params: { slug: string } }, parent: ResolvingMetadata): Promise<Metadata> {

    const slug = params.slug;
    // fetch data
    console.debug('FETCHING META', slug);

    const response = await fetch(process.env.NEXT_HYGRAPH_API_URL!, {
        next: {
            revalidate: 900,
        },
        method: 'POST',
        headers: {
            'content-type': 'application/json'
        },
        body: JSON.stringify({
            query: `query getPageMeta($slug:String!) {
                    page(where:{slug:$slug}) {
                        metadata {
                            title,
                            description,
                            image {
                                width,
                                height,
                                url
                            }
                        }
                    }
                  }`,
            variables: { slug }
        }),
    });
    const result = await response.json() as CmsMetaResponse;
    console.debug('PAGE META', result.data?.page?.metadata);

    return {
        title: result.data?.page?.metadata.title,
        description: result.data?.page?.metadata.description,
        openGraph: {
            images: [result.data?.page?.metadata.image?.url],
            title: result.data?.page?.metadata.title,
            description: result.data?.page?.metadata.description,
        },
    }
}