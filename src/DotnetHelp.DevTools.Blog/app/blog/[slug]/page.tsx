import { Metadata, ResolvingMetadata } from "next";
import CmsContentRender from "@/components/rich-text";
import FormattedDated from "@/components/date";
import Tags from "@/components/tags";

type BlogPost = {
    createdAt: string,
    slug: string,
    tags: string[],
    title: string,
    overview: string,
    category: string,
    body: {
        json: any
    }
}

type CmsResponse = {
    data: {
        blog: BlogPost
    }
};

type CmsBlogsResponse = {
    data: {
        blogs: {
            createdAt: string,
            slug: string,
            title: string,
            category: string
        }[]
    }
};

type CmsMetaResponse = {
    data: {
        blog: {
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

async function getPage(slug: string): Promise<BlogPost | null> {
    try {
        console.debug('FETCHING BLOG', slug);
        const response = await fetch(process.env.NEXT_HYGRAPH_API_URL!, {
            next: {
                revalidate: 900,
            },
            method: 'POST',
            headers: {
                'content-type': 'application/json'
            },
            body: JSON.stringify({
                query: `query getBlogPost($slug:String!) {
                    blog(where:{slug:$slug}) {
                      createdAt
                      slug
                      tags
                      title,
                      overview,
                      category,
                      body {
                        json
                      }
                    }
                  }`,
                variables: { slug }
            }),
        });
        const result = await response.json() as CmsResponse;
        console.debug('BLOG CONTENT', result);
        return result.data.blog;
    }
    catch (err) {
        console.error('ERROR DURING FETCH REQUEST', err);
    }

    return null;
}

export default async function Blog({ params }: { params: { slug: string } }) {
    const page = await getPage(params.slug);

    if (!page) {
        return <div>404</div>;
    }

    return <article>
        <header className="mb-5 md:flex md:items-start">
            <div className="flex-auto max-w-4xl">
                <h1 className="mb-4 text-sm leading-6 font-semibold text-sky-500 dark:text-sky-400">{page.title}</h1>
                <h2 className="text-3xl font-extrabold text-slate-900 tracking-tight dark:text-slate-200">{page.overview}</h2>
                <h2 className="text-xs mt-2 text-slate-900 tracking-tight dark:text-slate-200"><FormattedDated date={page.createdAt} /></h2>
            </div>
            <div>
                <Tags tags={page.tags} />
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
            query: `query Blogs {
                blogs {
                  slug
                }
              }`,
        }),
    });
    const result = await response.json() as CmsBlogsResponse;
    console.debug('ALL BLOGS', result.data.blogs);

    return result.data.blogs.map((post) => ({
        slug: post.slug,
    }))
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
            query: `query getBlogPostMeta($slug:String!) {
                    blog(where:{slug:$slug}) {
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
    console.debug('BLOG META', result.data.blog.metadata);

    return {
        title: result.data.blog.metadata.title,
        description: result.data.blog.metadata.description,
        openGraph: {
            images: [result.data.blog.metadata.image?.url],
            title: result.data.blog.metadata.title,
            description: result.data.blog.metadata.description,
        },
    }
}