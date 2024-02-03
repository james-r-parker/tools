import { BlogPost, PostOverview } from "@/components/overview";

type CmsResponse = {
    data: {
        blogs: BlogPost[]
    }
};

async function getPages(): Promise<BlogPost[]> {
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
                query: `query getBlogPosts {
                    blogs {
                      createdAt,
                      slug,
                      tags,
                      title,
                      overview,
                      category,
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
                  }`
            }),
        });
        const result = await response.json() as CmsResponse;
        console.debug('BLOG CONTENT', result);
        return result?.data?.blogs || [];
    }
    catch (err) {
        console.error('ERROR DURING FETCH REQUEST', err);
    }

    return [];
}


export default async function Blogs() {
    const pages = await getPages();

    return <article>
        <header className="mb-5 md:flex md:items-start">
            <div className="flex-auto max-w-4xl">
                <h1 className="mb-4 text-sm leading-6 font-semibold text-sky-500 dark:text-sky-400">Blog</h1>
                <h2 className="text-3xl sm:text-4xl font-extrabold text-slate-900 tracking-tight dark:text-slate-200">Blog Posts</h2>
            </div>
        </header>
        <div className="mt-5 grid gap-5 md:grid-cols-2">
            {pages.map((post, index) => <PostOverview key={index} post={post} />)}
        </div>
    </article>
}