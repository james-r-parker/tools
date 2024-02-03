import { RichText } from "@graphcms/rich-text-react-renderer";
import Link from "next/link";
import Image from "next/image";
import { Metadata, ResolvingMetadata } from "next";

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
                query: `query getBlogPostMeta($slug:String!) {
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
        console.debug('RESPONSE FROM FETCH REQUEST', result);
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
                <h2 className="text-3xl sm:text-4xl font-extrabold text-slate-900 tracking-tight dark:text-slate-200">{page.overview}</h2>
            </div>
        </header>
        <RichText
            content={page.body.json}
            renderers={{
                h1: ({ children }) => <h1 className="text-white text-3xl font-semibold my-2">{children}</h1>,
                h2: ({ children }) => <h1 className="text-white text-2xl font-semibold my-2">{children}</h1>,
                h3: ({ children }) => <h1 className="text-white text-xl font-semibold my-2">{children}</h1>,
                h4: ({ children }) => <h1 className="text-white text-xl">{children}</h1>,
                h5: ({ children }) => <h1 className="text-white text-lg">{children}</h1>,
                h6: ({ children }) => <h1 className="text-white">{children}</h1>,
                bold: ({ children }) => <span className="font-bold">{children}</span>,
                ul: ({ children }) => <ul className="list-disc list-inside my-4">{children}</ul>,
                ol: ({ children }) => <ol className="list-decimal list-inside my-4">{children}</ol>,
                a: ({ children, href, title }) => <Link className="text-sky-500 dark:text-sky-400 hover:underline" href={href!} title={title}>{children}</Link>,
                underline: ({ children }) => <span className="underline decoration-solid">{children}</span>,
                img: ({ src, width, height, altText }) => <Image src={src!} alt={altText || ""} width={width} height={height} />,
                code_block: ({ children }) => <pre className="bg-gray-800 p-4 rounded-md my-4"><code className="language-javascript">{children}</code></pre>,
                table: ({ children }) => <table className="table-auto w-full">{children}</table>,
                table_header_cell: ({ children }) => <th className="border-b dark:border-slate-600 font-medium p-4 pl-8 pt-0 pb-3 text-slate-400 dark:text-slate-200 text-left">{children}</th>,
                table_body: ({ children }) => <tbody className="bg-white dark:bg-slate-800">{children}</tbody>,
                table_cell: ({ children }) => <td className="border-b border-slate-100 dark:border-slate-700 p-4 pl-8 text-slate-500 dark:text-slate-400">{children}</td>,
            }}
        />
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
    console.debug('RESPONSE FROM FETCH REQUEST', result.data.blogs);

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
            query: `query getBlogPost($slug:String!) {
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
    console.debug('RESPONSE FROM FETCH META', result.data.blog.metadata);

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