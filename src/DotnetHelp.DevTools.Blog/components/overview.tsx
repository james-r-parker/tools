import Link from "next/link";
import Image from "next/image";
import Tags from "./tags";
import FormattedDated from "./date";

export type BlogPost = {
    createdAt: string,
    slug: string,
    tags: string[],
    title: string,
    overview: string,
    category: string,
    body: {
        json: any
    },
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

export function PostOverview({ post }: { post: BlogPost }) {
    return (
        <section className="w-full not-prose relative bg-slate-50 rounded-xl dark:bg-slate-800/25">
            <div style={{ backgroundPosition: "10px 10px" }} className="absolute inset-0 bg-grid-slate-100 [mask-image:linear-gradient(0deg,#fff,rgba(255,255,255,0.6))] dark:bg-grid-slate-700/25 dark:[mask-image:linear-gradient(0deg,rgba(255,255,255,0.1),rgba(255,255,255,0.5))]"></div>
            <div className="relative rounded-xl overflow-auto p-8">
                <div>
                    <div className="text-slate-900 dark:text-slate-200 text-wrap break-all">
                        <Link href={`/blog/${post.slug}`}>
                            <div className="flex gap-4">
                                <div>
                                    <Image src={post.metadata.image.url} alt={post.metadata.title} width={100} height={100} className="rounded-xl" />
                                </div>
                                <div>
                                    <h3 className="text-2xl font-semibold">{post.title}</h3>
                                    <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">{post.overview}</p>
                                    <p className="mt-1 text-xs text-slate-600 dark:text-slate-400"><FormattedDated date={post.createdAt} /></p>
                                    <div className="mt-1">
                                        <Tags tags={post.tags} />
                                    </div>
                                </div>
                            </div>
                        </Link>
                    </div>
                </div>
            </div>
            <div className="absolute inset-0 pointer-events-none border border-black/5 rounded-xl dark:border-white/5"></div>
        </section>
    )
}
